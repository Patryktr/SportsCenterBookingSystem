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
        return true;
    }
}