using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.TimeBlocks.CreateTimeBlock;

public class CreateTimeBlockRequest
{
    public int FacilityId { get; set; }
    public BlockType BlockType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
}