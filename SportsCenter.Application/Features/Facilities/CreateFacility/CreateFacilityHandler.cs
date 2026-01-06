using SportsCenter.Domain.Entities;
using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;

namespace SportsCenter.Application.Features.Facilities.CreateFacility;

public class CreateFacilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public CreateFacilityHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<CreateFacilityResponse> Handle(CreateFacilityRequest request, CancellationToken ct = default)
    {
        // Walidacja unikalnej nazwy (na wszelki wypadek — db już ma UniqueIndex)
        if (await _db.Facilities.AnyAsync(f => f.Name == request.Name,ct))
            throw new InvalidOperationException($"Facility '{request.Name}' already exists.");

        var facility = new Facility
        {
            Name = request.Name,
            SportType = request.SportType,
            MaxPlayers = request.MaxPlayers,
            PricePerHour = request.PricePerHour,
            IsActive = true
        };

        _db.Facilities.Add(facility);
        await _db.SaveChangesAsync(ct);

        return new CreateFacilityResponse(
            facility.Id,
            facility.Name,
            facility.SportType,
            facility.MaxPlayers,
            facility.PricePerHour,
            facility.IsActive
        );
    }
}