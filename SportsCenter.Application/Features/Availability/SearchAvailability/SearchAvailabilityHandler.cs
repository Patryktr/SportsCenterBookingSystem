using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;
    private readonly ILogger<SearchAvailabilityHandler> _logger;

    public SearchAvailabilityHandler(SportsCenterDbContext db, ILogger<SearchAvailabilityHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<SearchAvailabilityResponse>> Handle(SearchAvailabilityRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Wyszukiwanie dostępności dla obiektu {FacilityId} na dzień {Date}",
            request.FacilityId, request.Date.Date);

        // Pobierz obiekt
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        
        if (facility == null)
        {
            return Result<SearchAvailabilityResponse>.Failure("Obiekt sportowy nie istnieje");
        }

        if (!facility.IsActive)
        {
            return Result<SearchAvailabilityResponse>.Failure("Obiekt sportowy jest nieaktywny");
        }

        var searchDate = request.Date.Date;
        var dayOfWeek = (int)searchDate.DayOfWeek;

        // Pobierz godziny otwarcia dla tego dnia
        var operatingHours = await _db.Set<Domain.Entities.OperatingHours>()
            .FirstOrDefaultAsync(oh => oh.FacilityId == request.FacilityId && oh.DayOfWeek == (DayOfWeek)dayOfWeek, ct);

        // Domyślne godziny otwarcia jeśli nie zdefiniowano
        var openTime = operatingHours?.OpenTime ?? new TimeOnly(8, 0);
        var closeTime = operatingHours?.CloseTime ?? new TimeOnly(22, 0);
        var isClosed = operatingHours?.IsClosed ?? false;

        var response = new SearchAvailabilityResponse
        {
            FacilityId = facility.Id,
            FacilityName = facility.Name,
            Date = searchDate,
            PricePerHour = facility.PricePerHour,
            Slots = new List<TimeSlotItem>()
        };

        // Jeśli obiekt zamknięty w ten dzień
        if (isClosed)
        {
            response.Message = $"Obiekt jest zamknięty w dniu {searchDate:yyyy-MM-dd} ({GetPolishDayName(searchDate.DayOfWeek)})";
            response.TotalSlots = 0;
            response.AvailableSlots = 0;
            response.BookedSlots = 0;
            return Result<SearchAvailabilityResponse>.Success(response);
        }

        // Pobierz istniejące rezerwacje na ten dzień
        var dayStart = searchDate;
        var dayEnd = searchDate.AddDays(1);
        
        var existingBookings = await _db.Bookings
            .Where(b => b.FacilityId == request.FacilityId)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.Start < dayEnd && b.End > dayStart)
            .ToListAsync(ct);

        // Pobierz blokady na ten dzień
        var timeBlocks = await _db.Set<Domain.Entities.TimeBlock>()
            .Where(tb => tb.FacilityId == request.FacilityId)
            .Where(tb => tb.IsActive)
            .Where(tb => tb.StartTime < dayEnd && tb.EndTime > dayStart)
            .ToListAsync(ct);

        // Generuj sloty godzinowe
        var currentHour = openTime.Hour;
        var endHour = closeTime.Hour;
        var now = DateTime.UtcNow;

        while (currentHour < endHour)
        {
            var slotStart = searchDate.AddHours(currentHour);
            var slotEnd = searchDate.AddHours(currentHour + 1);

            var slot = new TimeSlotItem
            {
                Start = slotStart,
                End = slotEnd,
                StartTime = $"{currentHour:D2}:00",
                EndTime = $"{currentHour + 1:D2}:00"
            };

            // Sprawdź status slotu
            if (slotStart < now)
            {
                slot.Status = TimeSlotStatus.Past;
                slot.StatusName = "Minione";
            }
            else if (existingBookings.Any(b => b.Start < slotEnd && b.End > slotStart))
            {
                slot.Status = TimeSlotStatus.Booked;
                slot.StatusName = "Zarezerwowane";
            }
            else if (timeBlocks.Any(tb => tb.StartTime < slotEnd && tb.EndTime > slotStart))
            {
                var block = timeBlocks.First(tb => tb.StartTime < slotEnd && tb.EndTime > slotStart);
                slot.Status = TimeSlotStatus.Blocked;
                slot.StatusName = "Zablokowane";
                slot.BlockedReason = block.Reason ?? GetBlockTypeName(block.BlockType);
            }
            else
            {
                slot.Status = TimeSlotStatus.Available;
                slot.StatusName = "Wolne";
            }

            response.Slots.Add(slot);
            currentHour++;
        }

        response.TotalSlots = response.Slots.Count;
        response.AvailableSlots = response.Slots.Count(s => s.Status == TimeSlotStatus.Available);
        response.BookedSlots = response.Slots.Count(s => s.Status == TimeSlotStatus.Booked);

        if (response.TotalSlots == 0)
        {
            response.Message = "Brak dostępnych slotów dla podanych kryteriów";
        }
        else if (response.AvailableSlots == 0)
        {
            response.Message = "Wszystkie terminy są zajęte lub niedostępne";
        }
        else
        {
            response.Message = $"Znaleziono {response.AvailableSlots} wolnych terminów z {response.TotalSlots} dostępnych";
        }

        _logger.LogInformation(
            "Znaleziono {Available}/{Total} wolnych slotów dla obiektu {FacilityName} na {Date}",
            response.AvailableSlots, response.TotalSlots, facility.Name, searchDate);

        return Result<SearchAvailabilityResponse>.Success(response);
    }

    private static string GetPolishDayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Poniedziałek",
        DayOfWeek.Tuesday => "Wtorek",
        DayOfWeek.Wednesday => "Środa",
        DayOfWeek.Thursday => "Czwartek",
        DayOfWeek.Friday => "Piątek",
        DayOfWeek.Saturday => "Sobota",
        DayOfWeek.Sunday => "Niedziela",
        _ => day.ToString()
    };

    private static string GetBlockTypeName(BlockType blockType) => blockType switch
    {
        BlockType.Maintenance => "Przerwa techniczna",
        BlockType.SpecialEvent => "Wydarzenie specjalne",
        BlockType.Holiday => "Dzień wolny",
        BlockType.Other => "Inne",
        _ => blockType.ToString()
    };
}