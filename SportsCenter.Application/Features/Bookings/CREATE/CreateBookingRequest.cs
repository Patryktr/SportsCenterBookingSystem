using SportsCenter.Domain.Entities.Enums;
namespace SportsCenter.Application.Features.Bookings.CREATE;

public class CreateBookingRequest
{
    public int FacilityId { get; set; }
    public int CustomerId { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public int PlayersCount { get; set; }
    public BookingType Type { get; set; }
}