using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;

namespace SportsCenter.Application.Features.Facilities.GetFacilityById;

public class GetFacilityByIdHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public GetFacilityByIdHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<GetFacilityByIdResponse?> Handle(int id, CancellationToken ct = default)
    {
        var f = await _db.Facilities.FirstOrDefaultAsync(x => x.Id == id, ct);

        return f == null
            ? null
            : new GetFacilityByIdResponse(
                f.Id,
                f.Name,
                f.SportType,
                f.MaxPlayers,
                f.PricePerHour,
                f.IsActive,
                f.MinBookingDurationMinutes,
                f.MaxBookingDurationMinutes);
    }
}