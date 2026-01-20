using SportsCenter.Application.Common;

namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityResponse
{
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = default!;
    public DateTime Date { get; set; }
    public decimal PricePerHour { get; set; }
    public int TotalSlots { get; set; }
    public int AvailableSlots { get; set; }
    public int BookedSlots { get; set; }
    public string? Message { get; set; }
    public List<TimeSlotItem> Slots { get; set; } = new();
}