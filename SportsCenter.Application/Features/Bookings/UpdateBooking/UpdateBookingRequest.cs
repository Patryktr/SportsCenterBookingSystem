namespace SportsCenter.Application.Features.Bookings.UpdateBooking;

public class UpdateBookingRequest
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int PlayersCount { get; set; }
}