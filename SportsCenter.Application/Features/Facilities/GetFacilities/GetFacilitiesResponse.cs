using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Facilities.GetFacilities;

public record GetFacilitiesResponse(
    int Id,
    string Name,
    SportType SportType,
    int MaxPlayers,
    decimal PricePerHour,
    bool IsActive
);