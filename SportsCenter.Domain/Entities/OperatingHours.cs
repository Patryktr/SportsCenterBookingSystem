namespace SportsCenter.Domain.Entities;

public class OperatingHours
{
    public int Id { get; set; }
    
    public int FacilityId { get; set; }
    
    public DayOfWeek DayOfWeek { get; set; }
    
    public TimeOnly OpenTime { get; set; }
    
    public TimeOnly CloseTime { get; set; }
    
    public bool IsClosed { get; set; }
    
    public Facility Facility { get; set; } = default!;
}