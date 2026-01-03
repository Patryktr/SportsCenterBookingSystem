using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SportsCenter.Application.Features.Facilities.DeleteFacility;

public class DeleteFacilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public DeleteFacilityHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        
        var facility = await _db.Facilities.FindAsync(new object[] { id }, ct);

        if (facility == null)
            return false;

        _db.Facilities.Remove(facility);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}