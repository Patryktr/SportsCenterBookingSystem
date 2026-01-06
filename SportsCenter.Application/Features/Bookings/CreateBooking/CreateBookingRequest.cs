using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Bookings.CreateBooking;

public class CreateBookingRequest
{
    public int FacilityId { get; set; }
    public Guid CustomerPublicId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int PlayersCount { get; set; }
    public BookingType Type { get; set; } = BookingType.Exclusive;
}