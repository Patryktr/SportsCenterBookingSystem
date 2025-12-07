using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Facilities.CreateFacility;

public record CreateFacilityRequest(
    string Name,
    SportType SportType,
    int MaxPlayers,
    decimal PricePerHour
);