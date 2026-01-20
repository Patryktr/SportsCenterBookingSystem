using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Cache;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Facilities.UpdateFacility;

public class UpdateFacilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly ICacheService _cache;

    public UpdateFacilityHandler(SportsCenterDbContext db, ICacheService cache)
    {
        _db = db;
        _cache=cache;
        
    }

    public async Task<bool> Handle(UpdateFacilityRequest request, CancellationToken ct = default)
    {
        var facility = await _db.Facilities.FindAsync(new object[] { request.Id }, ct);

        if (facility == null)
            return false;

        // Walidacja min/max duration
        if (request.MinBookingDurationMinutes <= 0)
            throw new ArgumentException("Minimalna długość rezerwacji musi być większa niż 0");
        
        if (request.MaxBookingDurationMinutes <= 0)
            throw new ArgumentException("Maksymalna długość rezerwacji musi być większa niż 0");
        
        if (request.MinBookingDurationMinutes > request.MaxBookingDurationMinutes)
            throw new ArgumentException("Minimalna długość rezerwacji nie może być większa niż maksymalna");

        facility.Name = request.Name;
        facility.SportType = request.SportType;
        facility.MaxPlayers = request.MaxPlayers;
        facility.PricePerHour = request.PricePerHour;
        facility.MinBookingDurationMinutes = request.MinBookingDurationMinutes;
        facility.MaxBookingDurationMinutes = request.MaxBookingDurationMinutes;

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.FacilitiesAll);
        return true;
    }
}