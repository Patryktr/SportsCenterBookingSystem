using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Cache;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Facilities.DeleteFacility;

public class DeleteFacilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly ICacheService _cache;

    public DeleteFacilityHandler(SportsCenterDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> Handle(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        
        var facility = await _db.Facilities.FindAsync(new object[] { id }, ct);

        if (facility == null)
            return false;

        _db.Facilities.Remove(facility);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.FacilitiesAll);

        return true;
    }
}