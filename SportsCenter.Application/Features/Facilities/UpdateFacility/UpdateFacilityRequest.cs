using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Facilities.UpdateFacility;

public record UpdateFacilityRequest(
    int Id,
    string Name,
    SportType SportType,
    int MaxPlayers,
    decimal PricePerHour,
    int MinBookingDurationMinutes = 30,
    int MaxBookingDurationMinutes = 480
);