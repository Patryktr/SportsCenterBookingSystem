using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;

namespace SportsCenter.Application.Features.Facilities.UpdateFacility;

public class UpdateFacilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public UpdateFacilityHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(UpdateFacilityRequest request, CancellationToken ct = default)
    {
        var facility = await _db.Facilities.FindAsync(new object[] { request.Id }, ct);

        if (facility == null)
            return false;

        facility.Name = request.Name;
        facility.SportType = request.SportType;
        facility.MaxPlayers = request.MaxPlayers;
        facility.PricePerHour = request.PricePerHour;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}