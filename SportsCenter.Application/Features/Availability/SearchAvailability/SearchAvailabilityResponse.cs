using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityResponse
{
    public DateTime SearchStart { get; set; }
    public DateTime SearchEnd { get; set; }
    public int TotalAvailableFacilities { get; set; }
    public List<AvailableFacilityItem> AvailableFacilities { get; set; } = new();
}

public class AvailableFacilityItem
{
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = default!;
    public SportType SportType { get; set; }
    public string SportTypeName { get; set; } = default!;
    public int MaxPlayers { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal TotalPrice { get; set; }
    public int MinBookingDurationMinutes { get; set; }
    public int MaxBookingDurationMinutes { get; set; }
}