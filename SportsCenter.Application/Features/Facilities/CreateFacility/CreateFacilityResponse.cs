using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Facilities.CreateFacility;

public record CreateFacilityResponse(
    int Id,
    string Name,
    SportType SportType,
    int MaxPlayers,
    decimal PricePerHour,
    bool IsActive
);