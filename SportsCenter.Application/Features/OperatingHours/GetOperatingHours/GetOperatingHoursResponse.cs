using SportsCenter.Application.Common;

namespace SportsCenter.Application.Features.OperatingHours.GetOperatingHours;

public class GetOperatingHoursResponse
{
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = default!;
    public List<OperatingHoursItem> Schedule { get; set; } = new();
}