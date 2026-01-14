using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Application.Services;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly IAvailabilityService _availabilityService;
    private readonly ILogger<SearchAvailabilityHandler> _logger;

    public SearchAvailabilityHandler(
        SportsCenterDbContext db, 
        IAvailabilityService availabilityService,
        ILogger<SearchAvailabilityHandler> logger)
    {
        _db = db;
        _availabilityService = availabilityService;
        _logger = logger;
    }

    public async Task<Result<SearchAvailabilityResponse>> Handle(SearchAvailabilityRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Wyszukiwanie dostępności: {Start} - {End}, SportType: {SportType}, MinPlayers: {MinPlayers}",
            request.Start, request.End, request.SportType, request.MinPlayers);

        // Walidacja dat
        if (request.Start >= request.End)
        {
            return Result<SearchAvailabilityResponse>.Failure("Data rozpoczęcia musi być wcześniejsza niż data zakończenia");
        }

        if (request.Start < DateTime.UtcNow)
        {
            return Result<SearchAvailabilityResponse>.Failure("Nie można wyszukiwać dostępności w przeszłości");
        }

        // Pobierz wszystkie aktywne obiekty z opcjonalnymi filtrami
        var facilitiesQuery = _db.Facilities
            .Where(f => f.IsActive)
            .AsQueryable();

        if (request.SportType.HasValue)
        {
            facilitiesQuery = facilitiesQuery.Where(f => f.SportType == request.SportType.Value);
        }

        if (request.MinPlayers.HasValue)
        {
            facilitiesQuery = facilitiesQuery.Where(f => f.MaxPlayers >= request.MinPlayers.Value);
        }

        var facilities = await facilitiesQuery.ToListAsync(ct);

        // Sprawdź dostępność każdego obiektu
        var availableFacilities = new List<AvailableFacilityItem>();
        var duration = request.End - request.Start;
        var hours = (decimal)duration.TotalHours;

        foreach (var facility in facilities)
        {
            // Sprawdź minimalną i maksymalną długość rezerwacji
            var durationMinutes = (int)duration.TotalMinutes;
            if (durationMinutes < facility.MinBookingDurationMinutes)
            {
                _logger.LogDebug(
                    "Obiekt {FacilityName} pominięty - czas rezerwacji ({Duration} min) krótszy niż minimum ({Min} min)",
                    facility.Name, durationMinutes, facility.MinBookingDurationMinutes);
                continue;
            }

            if (durationMinutes > facility.MaxBookingDurationMinutes)
            {
                _logger.LogDebug(
                    "Obiekt {FacilityName} pominięty - czas rezerwacji ({Duration} min) dłuższy niż maksimum ({Max} min)",
                    facility.Name, durationMinutes, facility.MaxBookingDurationMinutes);
                continue;
            }

            var availabilityCheck = await _availabilityService.CheckAvailabilityAsync(
                facility.Id,
                request.Start,
                request.End,
                excludeBookingId: null,
                ct);

            if (availabilityCheck.IsAvailable)
            {
                availableFacilities.Add(new AvailableFacilityItem
                {
                    FacilityId = facility.Id,
                    FacilityName = facility.Name,
                    SportType = facility.SportType,
                    SportTypeName = GetPolishSportTypeName(facility.SportType),
                    MaxPlayers = facility.MaxPlayers,
                    PricePerHour = facility.PricePerHour,
                    TotalPrice = hours * facility.PricePerHour,
                    MinBookingDurationMinutes = facility.MinBookingDurationMinutes,
                    MaxBookingDurationMinutes = facility.MaxBookingDurationMinutes
                });
            }
        }

        _logger.LogInformation(
            "Znaleziono {Count} dostępnych obiektów w terminie {Start} - {End}",
            availableFacilities.Count, request.Start, request.End);

        return Result<SearchAvailabilityResponse>.Success(new SearchAvailabilityResponse
        {
            SearchStart = request.Start,
            SearchEnd = request.End,
            TotalAvailableFacilities = availableFacilities.Count,
            AvailableFacilities = availableFacilities
                .OrderBy(f => f.PricePerHour)
                .ToList()
        });
    }

    private static string GetPolishSportTypeName(SportType sportType) => sportType switch
    {
        SportType.Tennis => "Tenis",
        SportType.Football => "Piłka nożna",
        SportType.Padel => "Padel",
        SportType.Squash => "Squash",
        SportType.Badminton => "Badminton",
        _ => sportType.ToString()
    };
}