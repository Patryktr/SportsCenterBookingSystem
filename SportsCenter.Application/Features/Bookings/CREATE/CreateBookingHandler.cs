using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;
using System.Data;

using SportsCenter.Application.Features.Bookings.CREATE;


namespace SportsCenter.Application.Features.Bookings.CreateBooking;

public class CreateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public CreateBookingHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<CreateBookingResponse> Handle(CreateBookingRequest req, CancellationToken ct = default)
    {
        // 1) Walidacje czasu
        if (req.End <= req.Start)
            throw new InvalidOperationException("End must be after Start.");

        static bool IsOnHalfHourGrid(DateTime dt) =>
            dt.Second == 0 &&
            dt.Millisecond == 0 &&
            (dt.Minute == 0 || dt.Minute == 30);

        if (!IsOnHalfHourGrid(req.Start) || !IsOnHalfHourGrid(req.End))
            throw new InvalidOperationException("Start and End must be on 30-minute intervals (mm=00 or mm=30).");

        var duration = req.End - req.Start;

        if (duration < TimeSpan.FromMinutes(30))
            throw new InvalidOperationException("Booking must be at least 30 minutes.");

        if (duration.TotalMinutes % 30 != 0)
            throw new InvalidOperationException("Booking duration must be a multiple of 30 minutes.");

        // 2) Transakcja (ważne przy równoległych rezerwacjach)
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        // 3) Sprawdź facility
        var facility = await _db.Facilities
            .FirstOrDefaultAsync(f => f.Id == req.FacilityId && f.IsActive, ct);

        if (facility is null)
            throw new InvalidOperationException("Facility not found or inactive.");

        // 4) Sprawdź customer
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == req.CustomerId, ct);
        if (!customerExists)
            throw new InvalidOperationException("Customer not found.");

        // 5) Reguły zależne od typu
        int playersCountForBooking;

        switch (req.Type)
        {
            case BookingType.Exclusive:
                // przy wynajmie całej sali liczba graczy nie wpływa na dostępność
                playersCountForBooking = 1;
                break;

            case BookingType.GroupClass:
                if (req.PlayersCount < 1)
                    throw new InvalidOperationException("PlayersCount must be at least 1.");

                playersCountForBooking = req.PlayersCount;
                break;

            default:
                throw new InvalidOperationException("Unsupported booking type.");
        }

        // 6) Konflikty / dostępność
        if (req.Type == BookingType.Exclusive)
        {
            // Exclusive koliduje z KAŻDĄ aktywną rezerwacją na obiekcie
            var conflict = await _db.Bookings.AnyAsync(b =>
                b.FacilityId == req.FacilityId &&
                b.Status == BookingStatus.Active &&
                b.Start < req.End &&
                b.End > req.Start,
                ct);

            if (conflict)
                throw new InvalidOperationException("Facility already booked in this time.");
        }
        else // GroupClass
        {
            // GroupClass koliduje z aktywnym Exclusive (bo exclusive blokuje obiekt)
            var overlapsExclusive = await _db.Bookings.AnyAsync(b =>
                b.FacilityId == req.FacilityId &&
                b.Status == BookingStatus.Active &&
                b.Type == BookingType.Exclusive &&
                b.Start < req.End &&
                b.End > req.Start,
                ct);

            if (overlapsExclusive)
                throw new InvalidOperationException("Facility is exclusively booked in this time.");

            // Limit miejsc dla GroupClass (sumujemy zajęte miejsca w tym samym oknie czasu)
            var usedPlaces = await _db.Bookings
                .Where(b =>
                    b.FacilityId == req.FacilityId &&
                    b.Status == BookingStatus.Active &&
                    b.Type == BookingType.GroupClass &&
                    b.Start < req.End &&
                    b.End > req.Start)
                .SumAsync(b => (int?)b.PlayersCount, ct) ?? 0;

            if (usedPlaces + playersCountForBooking > facility.MaxPlayers)
                throw new InvalidOperationException("Not enough available places for this time.");
        }

        // 7) Cena
        var hours = duration.TotalMinutes / 60.0;
        var basePrice = (decimal)hours * facility.PricePerHour;

        // Jeśli PricePerHour jest "za obiekt", to dla GroupClass zwykle nie mnożysz przez liczbę osób.
        // Jeśli jest "za osobę", wtedy mnożysz.
        // Na start zostawiamy prostą regułę: Exclusive = basePrice, GroupClass = basePrice (łatwo zmienić).
        var totalPrice = basePrice;

        // 8) Zapis
        var booking = new Booking()
        {
            FacilityId = req.FacilityId,
            CustomerId = req.CustomerId,
            Start = req.Start,
            End = req.End,
            PlayersCount = playersCountForBooking,
            Type = req.Type,
            TotalPrice = totalPrice,
            Status = BookingStatus.Active
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new CreateBookingResponse
        {
            BookingId = booking.Id,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status
        };
    }
}
