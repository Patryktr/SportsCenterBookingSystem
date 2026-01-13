using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.TimeBlocks.CreateTimeBlock;

public class CreateTimeBlockHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public CreateTimeBlockHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CreateTimeBlockResponse>> Handle(CreateTimeBlockRequest request, CancellationToken ct = default)
    {
        // Walidacja dat
        if (request.StartTime >= request.EndTime)
        {
            return Result<CreateTimeBlockResponse>.Failure("Data rozpoczęcia musi być wcześniejsza niż data zakończenia");
        }

        // Sprawdź czy obiekt istnieje
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        if (facility == null)
        {
            return Result<CreateTimeBlockResponse>.Failure("Obiekt sportowy nie istnieje");
        }

        // Sprawdź czy nie ma nakładających się blokad
        var hasOverlappingBlock = await _db.TimeBlocks
            .Where(t => t.FacilityId == request.FacilityId)
            .Where(t => t.IsActive)
            .Where(t => t.StartTime < request.EndTime && t.EndTime > request.StartTime)
            .AnyAsync(ct);

        if (hasOverlappingBlock)
        {
            return Result<CreateTimeBlockResponse>.Failure("Istnieje już blokada w podanym przedziale czasowym");
        }

        var timeBlock = new Domain.Entities.TimeBlock
        {
            FacilityId = request.FacilityId,
            BlockType = request.BlockType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Reason = request.Reason,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimeBlocks.Add(timeBlock);
        await _db.SaveChangesAsync(ct);

        return Result<CreateTimeBlockResponse>.Success(new CreateTimeBlockResponse
        {
            Id = timeBlock.Id,
            FacilityId = facility.Id,
            FacilityName = facility.Name,
            BlockType = timeBlock.BlockType,
            BlockTypeName = GetPolishBlockTypeName(timeBlock.BlockType),
            StartTime = timeBlock.StartTime,
            EndTime = timeBlock.EndTime,
            Reason = timeBlock.Reason,
            IsActive = timeBlock.IsActive,
            CreatedAt = timeBlock.CreatedAt
        });
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