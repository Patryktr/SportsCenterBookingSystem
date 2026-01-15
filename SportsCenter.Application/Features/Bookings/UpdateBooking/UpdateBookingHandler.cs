using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Application.Services;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.UpdateBooking;

public class UpdateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly IAvailabilityService _availabilityService;

    public UpdateBookingHandler(SportsCenterDbContext db, IAvailabilityService availabilityService)
    {
        _db = db;
        _availabilityService = availabilityService;
    }

    public async Task<Result<bool>> Handle(int id, UpdateBookingRequest request, CancellationToken ct = default)
    {
        // Walidacja pełnych godzin
        if (!IsFullHour(request.Start))
            return Result<bool>.Failure(
                "Rezerwacja musi rozpoczynać się o pełnej godzinie (np. 10:00, 11:00)");
        
        if (!IsFullHour(request.End))
            return Result<bool>.Failure(
                "Rezerwacja musi kończyć się o pełnej godzinie (np. 10:00, 11:00)");

        var booking = await _db.Bookings
            .Include(b => b.Facility)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking == null)
            return Result<bool>.Failure("Rezerwacja nie istnieje");

        if (booking.Status == BookingStatus.Canceled)
            return Result<bool>.Failure("Nie można edytować anulowanej rezerwacji");

        // Walidacja dat
        if (request.Start >= request.End)
            return Result<bool>.Failure("Data rozpoczęcia musi być wcześniejsza niż data zakończenia");

        if (request.Start < DateTime.UtcNow)
            return Result<bool>.Failure("Nie można zarezerwować w przeszłości");

        // Walidacja liczby graczy
        if (request.PlayersCount < 1)
            return Result<bool>.Failure("Liczba graczy musi być większa niż 0");

        if (request.PlayersCount > booking.Facility.MaxPlayers)
            return Result<bool>.Failure($"Maksymalna liczba graczy dla tego obiektu to {booking.Facility.MaxPlayers}");

        // Walidacja długości rezerwacji
        var durationMinutes = (request.End - request.Start).TotalMinutes;
        
        if (durationMinutes < booking.Facility.MinBookingDurationMinutes)
            return Result<bool>.Failure(
                $"Minimalna długość rezerwacji dla tego obiektu to {booking.Facility.MinBookingDurationMinutes} minut");
        
        if (durationMinutes > booking.Facility.MaxBookingDurationMinutes)
            return Result<bool>.Failure(
                $"Maksymalna długość rezerwacji dla tego obiektu to {booking.Facility.MaxBookingDurationMinutes} minut ({booking.Facility.MaxBookingDurationMinutes / 60} godzin)");

        // Sprawdź dostępność (wykluczając tę samą rezerwację)
        var availabilityCheck = await _availabilityService.CheckAvailabilityAsync(
            booking.FacilityId,
            request.Start,
            request.End,
            excludeBookingId: id,
            ct);

        if (!availabilityCheck.IsAvailable)
        {
            return Result<bool>.Failure(availabilityCheck.ConflictMessage ?? "Obiekt jest niedostępny w wybranym terminie");
        }

        // Przelicz cenę
        var duration = request.End - request.Start;
        var hours = (decimal)duration.TotalHours;
        var totalPrice = hours * booking.Facility.PricePerHour;

        // Aktualizuj rezerwację
        booking.Start = request.Start;
        booking.End = request.End;
        booking.PlayersCount = request.PlayersCount;
        booking.TotalPrice = totalPrice;

        await _db.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
    
    /// Sprawdza czy podany czas to pełna godzina (minuty i sekundy = 0)
    private static bool IsFullHour(DateTime dateTime)
    {
        return dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
    }
}