using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SportsCenter.Application.Features.Facilities.UpdateFacility;

public class UpdateFacilityHandler
{
    private readonly SportsCenterDbContext _db;

    public UpdateFacilityHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(UpdateFacilityRequest request, CancellationToken ct)
    {
        var facility = await _db.Facilities.FindAsync(request.Id);

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