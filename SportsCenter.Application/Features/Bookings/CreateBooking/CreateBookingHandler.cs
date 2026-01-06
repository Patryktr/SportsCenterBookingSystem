using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;
using SportsCenter.Application.Common;

namespace SportsCenter.Application.Features.Bookings.CreateBooking;

public class CreateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public CreateBookingHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingRequest request, CancellationToken ct)
    {
        // Walidacja dat
        if (request.Start >= request.End)
            return Result<CreateBookingResponse>.Failure("Data rozpoczęcia musi być wcześniejsza niż data zakończenia");

        if (request.Start < DateTime.UtcNow)
            return Result<CreateBookingResponse>.Failure("Nie można zarezerwować w przeszłości");

        // Sprawdź czy facility istnieje i jest aktywne
        var facility = await _db.Facilities.FindAsync(request.FacilityId);
        if (facility == null)
            return Result<CreateBookingResponse>.Failure("Obiekt sportowy nie istnieje");

        if (!facility.IsActive)
            return Result<CreateBookingResponse>.Failure("Obiekt sportowy jest nieaktywny");

        // Sprawdź czy customer istnieje
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.PublicId == request.CustomerPublicId);
        
        if (customer == null)
            return Result<CreateBookingResponse>.Failure("Klient nie istnieje");

        // Walidacja liczby graczy
        if (request.PlayersCount < 1)
            return Result<CreateBookingResponse>.Failure("Liczba graczy musi być większa niż 0");

        if (request.PlayersCount > facility.MaxPlayers)
            return Result<CreateBookingResponse>.Failure($"Maksymalna liczba graczy dla tego obiektu to {facility.MaxPlayers}");

        // Sprawdź dostępność (czy nie ma nakładających się rezerwacji)
        var hasConflict = await _db.Bookings
            .Where(b => b.FacilityId == request.FacilityId)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => 
                (b.Start < request.End && b.End > request.Start)) // Nakładające się przedziały czasowe
            .AnyAsync();

        if (hasConflict)
            return Result<CreateBookingResponse>.Failure("Ten obiekt jest już zarezerwowany w wybranym terminie");

        // Oblicz cenę
        var duration = request.End - request.Start;
        var hours = (decimal)duration.TotalHours;
        var totalPrice = hours * facility.PricePerHour;

        // Utwórz rezerwację
        var booking = new Booking
        {
            FacilityId = request.FacilityId,
            CustomerId = customer.Id,
            Start = request.Start,
            End = request.End,
            PlayersCount = request.PlayersCount,
            TotalPrice = totalPrice,
            Status = BookingStatus.Active,
            Type = request.Type
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Załaduj relacje dla odpowiedzi
        await _db.Entry(booking)
            .Reference(b => b.Facility)
            .LoadAsync();
        await _db.Entry(booking)
            .Reference(b => b.Customer)
            .LoadAsync();

        return Result<CreateBookingResponse>.Success(new CreateBookingResponse
        {
            Id = booking.Id,
            FacilityId = booking.FacilityId,
            FacilityName = booking.Facility.Name,
            CustomerPublicId = booking.Customer.PublicId,
            CustomerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}",
            Start = booking.Start,
            End = booking.End,
            PlayersCount = booking.PlayersCount,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            Type = booking.Type
        });
    }
}