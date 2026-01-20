using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Cache;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Facilities.GetFacilities;

public class GetFacilitiesHandler : IHandlerDefinition
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    private readonly SportsCenterDbContext _db;
    private readonly ICacheService _cache;

    public GetFacilitiesHandler(
        SportsCenterDbContext db,
        ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public Task<IEnumerable<GetFacilitiesResponse>> Handle(CancellationToken ct)
    {
        return _cache.GetOrCreateAsync<IEnumerable<GetFacilitiesResponse>>(
            CacheKeys.FacilitiesAll,
            async _ =>
            {
                return await _db.Facilities
                    .AsNoTracking()
                    .Select(f => new GetFacilitiesResponse(
                        f.Id,
                        f.Name,
                        f.SportType,
                        f.MaxPlayers,
                        f.PricePerHour,
                        f.IsActive,
                        f.MinBookingDurationMinutes,
                        f.MaxBookingDurationMinutes
                    ))
                    .ToListAsync(ct);
            },
            ttl: CacheTtl,
            ct: ct);
    }

}