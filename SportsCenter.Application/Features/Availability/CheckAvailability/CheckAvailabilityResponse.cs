namespace SportsCenter.Application.Features.Availability.CheckAvailability;

public class CheckAvailabilityResponse
{
    public bool IsAvailable { get; set; }
    public int FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? ConflictReason { get; set; }
}