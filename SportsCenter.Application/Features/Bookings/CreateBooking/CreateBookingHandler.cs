using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Application.Services;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.CreateBooking;

public class CreateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly IAvailabilityService _availabilityService;

    public CreateBookingHandler(SportsCenterDbContext db, IAvailabilityService availabilityService)
    {
        _db = db;
        _availabilityService = availabilityService;
    }

    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingRequest request, CancellationToken ct)
    {
        // Walidacja dat
        if (request.Start >= request.End)
            return Result<CreateBookingResponse>.Failure("Data rozpoczęcia musi być wcześniejsza niż data zakończenia");

        if (request.Start < DateTime.UtcNow)
            return Result<CreateBookingResponse>.Failure("Nie można zarezerwować w przeszłości");

        // Sprawdź czy facility istnieje
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        if (facility == null)
            return Result<CreateBookingResponse>.Failure("Obiekt sportowy nie istnieje");

        // Sprawdź czy customer istnieje
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.PublicId == request.CustomerPublicId, ct);
        
        if (customer == null)
            return Result<CreateBookingResponse>.Failure("Klient nie istnieje");

        // Walidacja liczby graczy
        if (request.PlayersCount < 1)
            return Result<CreateBookingResponse>.Failure("Liczba graczy musi być większa niż 0");

        if (request.PlayersCount > facility.MaxPlayers)
            return Result<CreateBookingResponse>.Failure($"Maksymalna liczba graczy dla tego obiektu to {facility.MaxPlayers}");

        // Sprawdź dostępność (godziny otwarcia, blokady, istniejące rezerwacje)
        var availabilityCheck = await _availabilityService.CheckAvailabilityAsync(
            request.FacilityId,
            request.Start,
            request.End,
            excludeBookingId: null,
            ct);

        if (!availabilityCheck.IsAvailable)
        {
            return Result<CreateBookingResponse>.Failure(availabilityCheck.ConflictMessage ?? "Obiekt jest niedostępny w wybranym terminie");
        }

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
        await _db.SaveChangesAsync(ct);

        // Załaduj relacje dla odpowiedzi
        await _db.Entry(booking)
            .Reference(b => b.Facility)
            .LoadAsync(ct);
        await _db.Entry(booking)
            .Reference(b => b.Customer)
            .LoadAsync(ct);

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