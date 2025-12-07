using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SportsCenter.Application.Features.Facilities.GetFacilities;

public class GetFacilitiesHandler
{
    private readonly SportsCenterDbContext _db;

    public GetFacilitiesHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<GetFacilitiesResponse>> Handle(CancellationToken ct)
    {
        return await _db.Facilities
            .AsNoTracking()
            .Select(f => new GetFacilitiesResponse(
                f.Id,
                f.Name,
                f.SportType,
                f.MaxPlayers,
                f.PricePerHour,
                f.IsActive))
            .ToListAsync(ct);
    }
}