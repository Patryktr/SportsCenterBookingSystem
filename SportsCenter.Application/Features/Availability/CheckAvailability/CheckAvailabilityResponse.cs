using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Availability.CheckAvailability;

public class CheckAvailabilityResponse
{
    public bool IsAvailable { get; set; }
    public AvailabilityConflictType? ConflictType { get; set; }
    public string? ConflictMessage { get; set; }
    public int FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}