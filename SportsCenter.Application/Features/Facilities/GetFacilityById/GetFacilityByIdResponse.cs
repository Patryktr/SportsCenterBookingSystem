using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Facilities.GetFacilityById;

public record GetFacilityByIdResponse(
    int Id,
    string Name,
    SportType SportType,
    int MaxPlayers,
    decimal PricePerHour,
    bool IsActive,
    int MinBookingDurationMinutes,
    int MaxBookingDurationMinutes
);