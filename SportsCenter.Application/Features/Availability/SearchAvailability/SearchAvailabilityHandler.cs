using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public SearchAvailabilityHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SearchAvailabilityResponse>> Handle(SearchAvailabilityRequest request, CancellationToken ct = default)
    {
        // ========== WALIDACJA FACILITY ==========
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        
        if (facility == null)
            return Result<SearchAvailabilityResponse>.Failure("Obiekt sportowy nie istnieje");

        if (!facility.IsActive)
            return Result<SearchAvailabilityResponse>.Failure("Obiekt sportowy jest nieaktywny");

        var searchDate = request.Date.Date;
        var dayOfWeek = searchDate.DayOfWeek;

        // ========== POBRANIE GODZIN OTWARCIA ==========
        var operatingHours = await _db.OperatingHours
            .FirstOrDefaultAsync(oh => oh.FacilityId == request.FacilityId && oh.DayOfWeek == dayOfWeek, ct);

        // Domyślne godziny otwarcia jeśli nie zdefiniowano
        var openHour = operatingHours?.OpenTime.Hour ?? 8;
        var closeHour = operatingHours?.CloseTime.Hour ?? 22;
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
            response.Message = $"Obiekt jest zamknięty w dniu {searchDate:yyyy-MM-dd} ({GetPolishDayName(dayOfWeek)})";
            response.TotalSlots = 0;
            response.AvailableSlots = 0;
            response.BookedSlots = 0;
            return Result<SearchAvailabilityResponse>.Success(response);
        }

        // ========== POBRANIE ISTNIEJĄCYCH REZERWACJI ==========
        var dayStart = searchDate;
        var dayEnd = searchDate.AddDays(1);
        
        var existingBookings = await _db.Bookings
            .Where(b => b.FacilityId == request.FacilityId)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.Start < dayEnd && b.End > dayStart)
            .ToListAsync(ct);

        // ========== POBRANIE BLOKAD ==========
        var timeBlocks = await _db.TimeBlocks
            .Where(tb => tb.FacilityId == request.FacilityId)
            .Where(tb => tb.IsActive)
            .Where(tb => tb.StartTime < dayEnd && tb.EndTime > dayStart)
            .ToListAsync(ct);

        // ========== GENEROWANIE SLOTÓW GODZINOWYCH ==========
        var now = DateTime.UtcNow;

        for (var hour = openHour; hour < closeHour; hour++)
        {
            var slotStart = searchDate.AddHours(hour);
            var slotEnd = searchDate.AddHours(hour + 1);

            var slot = new TimeSlotItem
            {
                Start = slotStart,
                End = slotEnd,
                StartTime = $"{hour:D2}:00",
                EndTime = $"{hour + 1:D2}:00"
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
                slot.Status = TimeSlotStatus.Blocked;
                slot.StatusName = "Zablokowane";
            }
            else
            {
                slot.Status = TimeSlotStatus.Available;
                slot.StatusName = "Wolne";
            }

            response.Slots.Add(slot);
        }

        // ========== PODSUMOWANIE ==========
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
}