namespace SportsCenter.Application.Features.OperatingHours.SetOperatingHours;

public class SetOperatingHoursRequest
{
    public int FacilityId { get; set; }
    public List<DayOperatingHours> Schedule { get; set; } = new();
}

public class DayOperatingHours
{
    public DayOfWeek DayOfWeek { get; set; }
    
    /// <summary>
    /// Godzina otwarcia w formacie HH:mm (np. "08:00")
    /// </summary>
    public string? OpenTime { get; set; }
    
    /// <summary>
    /// Godzina zamknięcia w formacie HH:mm (np. "22:00")
    /// </summary>
    public string? CloseTime { get; set; }
    
    /// <summary>
    /// Czy obiekt jest zamknięty w tym dniu
    /// </summary>
    public bool IsClosed { get; set; }
}