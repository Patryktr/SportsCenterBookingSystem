using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityRequest
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public SportType? SportType { get; set; }
    public int? MinPlayers { get; set; }
}