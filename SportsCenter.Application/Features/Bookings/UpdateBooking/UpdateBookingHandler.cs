using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.UpdateBooking;

public class UpdateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public UpdateBookingHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(int id, UpdateBookingRequest request, CancellationToken ct = default)
    {
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

        // Sprawdź dostępność (wykluczając tę samą rezerwację)
        var hasConflict = await _db.Bookings
            .Where(b => b.FacilityId == booking.FacilityId)
            .Where(b => b.Id != id) // Wykluczamy tę samą rezerwację
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => 
                (b.Start < request.End && b.End > request.Start))
            .AnyAsync();

        if (hasConflict)
            return Result<bool>.Failure("Ten obiekt jest już zarezerwowany w wybranym terminie");

        // Przelicz cenę
        var duration = request.End - request.Start;
        var hours = (decimal)duration.TotalHours;
        var totalPrice = hours * booking.Facility.PricePerHour;

        // Aktualizuj rezerwację
        booking.Start = request.Start;
        booking.End = request.End;
        booking.PlayersCount = request.PlayersCount;
        booking.TotalPrice = totalPrice;

        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}