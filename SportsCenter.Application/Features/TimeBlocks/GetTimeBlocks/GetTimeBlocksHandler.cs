using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.TimeBlocks.GetTimeBlocks;

public class GetTimeBlocksHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public GetTimeBlocksHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<GetTimeBlocksResponse>> Handle(int? facilityId, bool? activeOnly, CancellationToken ct = default)
    {
        var query = _db.TimeBlocks
            .Include(t => t.Facility)
            .AsQueryable();

        if (facilityId.HasValue)
        {
            query = query.Where(t => t.FacilityId == facilityId.Value);
        }

        if (activeOnly == true)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderByDescending(t => t.StartTime)
            .Select(t => new GetTimeBlocksResponse
            {
                Id = t.Id,
                FacilityId = t.FacilityId,
                FacilityName = t.Facility.Name,
                BlockType = t.BlockType,
                BlockTypeName = GetPolishBlockTypeName(t.BlockType),
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                Reason = t.Reason,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(ct);
    }

    private static string GetPolishBlockTypeName(BlockType blockType) => blockType switch
    {
        BlockType.Maintenance => "Przerwa techniczna",
        BlockType.SpecialEvent => "Wydarzenie specjalne",
        BlockType.Holiday => "Dzień wolny",
        BlockType.Other => "Inne",
        _ => blockType.ToString()
    };
}