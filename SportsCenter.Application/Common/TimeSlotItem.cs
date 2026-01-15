using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Common;

public class TimeSlotItem
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string StartTime { get; set; } = default!;  // np. "10:00"
    public string EndTime { get; set; } = default!;    // np. "11:00"
    public TimeSlotStatus Status { get; set; }
    public string StatusName { get; set; } = default!; // "Wolne" / "Zarezerwowane" / "Zamknięte"
    public string? BlockedReason { get; set; }         // Powód blokady jeśli dotyczy
}