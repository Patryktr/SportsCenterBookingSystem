using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Bookings.GetBookings;

public class GetBookingsResponse
{
    public int Id { get; set; }
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = default!;
    public SportType SportType { get; set; }
    public Guid CustomerPublicId { get; set; }
    public string CustomerName { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int PlayersCount { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public BookingType Type { get; set; }
}