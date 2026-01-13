using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.TimeBlocks.DeleteTimeBlock;

public class DeleteTimeBlockHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public DeleteTimeBlockHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(int id, CancellationToken ct = default)
    {
        var timeBlock = await _db.TimeBlocks.FindAsync(new object[] { id }, ct);

        if (timeBlock == null)
        {
            return false;
        }

        _db.TimeBlocks.Remove(timeBlock);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}