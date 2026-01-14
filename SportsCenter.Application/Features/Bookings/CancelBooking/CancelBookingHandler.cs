using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportsCenter.Application.Abstractions;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.CancelBooking;

public class CancelBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly ILogger<CancelBookingHandler> _logger;
    
    private const int MinHoursBeforeCancellation = 1;

    public CancelBookingHandler(SportsCenterDbContext db, ILogger<CancelBookingHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CancelBookingResponse> Handle(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Rozpoczęto anulowanie rezerwacji {BookingId}", id);
        
        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Facility)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking == null)
        {
            _logger.LogWarning("Próba anulowania nieistniejącej rezerwacji {BookingId}", id);
            return new CancelBookingResponse
            {
                BookingId = id,
                Result = CancellationResult.NotFound,
                Message = "Rezerwacja nie została znaleziona"
            };
        }

        // Idempotencja: rezerwacja już anulowana
        if (booking.Status == BookingStatus.Canceled)
        {
            _logger.LogInformation("Rezerwacja {BookingId} była już wcześniej anulowana", id);
            return new CancelBookingResponse
            {
                BookingId = id,
                Result = CancellationResult.AlreadyCancelled,
                Message = "Rezerwacja była już wcześniej anulowana"
            };
        }

        // Sprawdź czy nie jest za późno na anulowanie
        var timeUntilStart = booking.Start - DateTime.UtcNow;
        if (timeUntilStart.TotalHours < MinHoursBeforeCancellation)
        {
            _logger.LogWarning(
                "Próba anulowania rezerwacji {BookingId} zbyt późno. Czas do rozpoczęcia: {MinutesUntilStart} minut",
                id, timeUntilStart.TotalMinutes);
            
            return new CancelBookingResponse
            {
                BookingId = id,
                Result = CancellationResult.TooLateToCancel,
                Message = $"Nie można anulować rezerwacji na mniej niż {MinHoursBeforeCancellation} godzinę przed rozpoczęciem. " +
                          $"Rezerwacja rozpoczyna się {booking.Start:yyyy-MM-dd HH:mm}"
            };
        }

        // Anuluj rezerwację
        booking.Status = BookingStatus.Canceled;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Rezerwacja {BookingId} została anulowana. Klient: {CustomerName}, Obiekt: {FacilityName}",
            id,
            $"{booking.Customer.FirstName} {booking.Customer.LastName}",
            booking.Facility.Name);

        return new CancelBookingResponse
        {
            BookingId = id,
            Result = CancellationResult.Success,
            Message = "Rezerwacja została pomyślnie anulowana",
            CancelledAt = DateTime.UtcNow
        };
    }
}