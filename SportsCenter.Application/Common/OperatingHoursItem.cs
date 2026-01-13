namespace SportsCenter.Application.Common;

public class OperatingHoursItem
{
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = default!;
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}