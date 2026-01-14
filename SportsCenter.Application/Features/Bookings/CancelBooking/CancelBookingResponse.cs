using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Bookings.CancelBooking;

public class CancelBookingResponse
{
    public int BookingId { get; set; }
    public CancellationResult Result { get; set; }
    public string Message { get; set; } = default!;
    public DateTime? CancelledAt { get; set; }
}