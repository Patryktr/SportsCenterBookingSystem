using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Common;

public class TimeSlotItem
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
    public TimeSlotStatus Status { get; set; }
    public string StatusName { get; set; } = default!;
}