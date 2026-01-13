using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Domain.Entities;

public class TimeBlock
{
    public int Id { get; set; }
    
    public int FacilityId { get; set; }
    
    public BlockType BlockType { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public string? Reason { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Facility Facility { get; set; } = default!;
}