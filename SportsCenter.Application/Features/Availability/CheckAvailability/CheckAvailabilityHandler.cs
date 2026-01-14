using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Features.Availability.CheckAvailability;
using SportsCenter.Application.Services;
using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Availability.CheckAvailability;

public class CheckAvailabilityHandler : IHandlerDefinition
{
    private readonly IAvailabilityService _availabilityService;

    public CheckAvailabilityHandler(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    public async Task<CheckAvailabilityResponse> Handle(CheckAvailabilityRequest request, CancellationToken ct = default)
    {
        var result = await _availabilityService.CheckAvailabilityAsync(
            request.FacilityId,
            request.Start,
            request.End,
            excludeBookingId: null,
            ct);

        return new CheckAvailabilityResponse
        {
            IsAvailable = result.IsAvailable,
            ConflictType = result.IsAvailable ? null : result.ConflictType,
            ConflictMessage = result.ConflictMessage,
            FacilityId = request.FacilityId,
            Start = request.Start,
            End = request.End
        };
    }
}