using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Bookings.GetBookingById;

public class GetBookingByIdResponse
{
    public int Id { get; set; }
    
    // Facility info
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = default!;
    public SportType SportType { get; set; }
    public decimal FacilityPricePerHour { get; set; }
    
    // Customer info
    public Guid CustomerPublicId { get; set; }
    public string CustomerFirstName { get; set; } = default!;
    public string CustomerLastName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string? CustomerPhone { get; set; }
    
    // Booking details
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int PlayersCount { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public BookingType Type { get; set; }
}