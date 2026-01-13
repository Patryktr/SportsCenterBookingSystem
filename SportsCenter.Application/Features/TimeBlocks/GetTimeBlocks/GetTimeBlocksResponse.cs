using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.TimeBlocks.GetTimeBlocks;

public class GetTimeBlocksResponse
{
    public int Id { get; set; }
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = default!;
    public BlockType BlockType { get; set; }
    public string BlockTypeName { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}