using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Cache;
using SportsCenter.Domain.Entities;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Facilities.CreateFacility;

public class CreateFacilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly ICacheService _cache;

    public CreateFacilityHandler(SportsCenterDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<CreateFacilityResponse> Handle(CreateFacilityRequest request, CancellationToken ct = default)
    {
        // Walidacja unikalnej nazwy
        if (await _db.Facilities.AnyAsync(f => f.Name == request.Name, ct))
            throw new InvalidOperationException($"Obiekt o nazwie '{request.Name}' już istnieje.");

        // Walidacja min/max duration
        if (request.MinBookingDurationMinutes <= 0)
            throw new ArgumentException("Minimalna długość rezerwacji musi być większa niż 0");
        
        if (request.MaxBookingDurationMinutes <= 0)
            throw new ArgumentException("Maksymalna długość rezerwacji musi być większa niż 0");
        
        if (request.MinBookingDurationMinutes > request.MaxBookingDurationMinutes)
            throw new ArgumentException("Minimalna długość rezerwacji nie może być większa niż maksymalna");

        var facility = new Facility
        {
            Name = request.Name,
            SportType = request.SportType,
            MaxPlayers = request.MaxPlayers,
            PricePerHour = request.PricePerHour,
            IsActive = true,
            MinBookingDurationMinutes = request.MinBookingDurationMinutes,
            MaxBookingDurationMinutes = request.MaxBookingDurationMinutes
        };

        _db.Facilities.Add(facility);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.FacilitiesAll);


        return new CreateFacilityResponse(
            facility.Id,
            facility.Name,
            facility.SportType,
            facility.MaxPlayers,
            facility.PricePerHour,
            facility.IsActive,
            facility.MinBookingDurationMinutes,
            facility.MaxBookingDurationMinutes
        );
    }
}