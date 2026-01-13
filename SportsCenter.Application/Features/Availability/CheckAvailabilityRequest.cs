namespace SportsCenter.Application.Features.Availability;

public class CheckAvailabilityRequest
{
    public int FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}