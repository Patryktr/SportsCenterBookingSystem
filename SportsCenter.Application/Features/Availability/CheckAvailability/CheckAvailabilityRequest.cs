namespace SportsCenter.Application.Features.Availability.CheckAvailability;

public class CheckAvailabilityRequest
{
    public int FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}