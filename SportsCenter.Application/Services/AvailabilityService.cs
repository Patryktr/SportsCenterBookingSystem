using Microsoft.EntityFrameworkCore;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Services;

public record AvailabilityCheckResult(
    bool IsAvailable,
    AvailabilityConflictType ConflictType,
    string? ConflictMessage
);

public interface IAvailabilityService
{
    Task<AvailabilityCheckResult> CheckAvailabilityAsync(
        int facilityId,
        DateTime start,
        DateTime end,
        int? excludeBookingId = null,
        CancellationToken ct = default);
}

public class AvailabilityService : IAvailabilityService
{
    private readonly SportsCenterDbContext _db;

    public AvailabilityService(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<AvailabilityCheckResult> CheckAvailabilityAsync(
        int facilityId,
        DateTime start,
        DateTime end,
        int? excludeBookingId = null,
        CancellationToken ct = default)
    {
        // 1. Sprawdź czy obiekt istnieje i jest aktywny
        var facility = await _db.Facilities.FindAsync(new object[] { facilityId }, ct);
        if (facility == null)
        {
            return new AvailabilityCheckResult(
                false,
                AvailabilityConflictType.FacilityInactive,
                "Obiekt sportowy nie istnieje");
        }

        if (!facility.IsActive)
        {
            return new AvailabilityCheckResult(
                false,
                AvailabilityConflictType.FacilityInactive,
                "Obiekt sportowy jest nieaktywny");
        }

        // 2. Sprawdź godziny otwarcia dla każdego dnia w zakresie rezerwacji
        var operatingHoursCheck = await CheckOperatingHoursAsync(facilityId, start, end, ct);
        if (!operatingHoursCheck.IsAvailable)
        {
            return operatingHoursCheck;
        }

        // 3. Sprawdź blokady terminów
        var timeBlockCheck = await CheckTimeBlocksAsync(facilityId, start, end, ct);
        if (!timeBlockCheck.IsAvailable)
        {
            return timeBlockCheck;
        }

        // 4. Sprawdź istniejące rezerwacje
        var bookingCheck = await CheckExistingBookingsAsync(facilityId, start, end, excludeBookingId, ct);
        if (!bookingCheck.IsAvailable)
        {
            return bookingCheck;
        }

        return new AvailabilityCheckResult(true, AvailabilityConflictType.None, null);
    }

    private async Task<AvailabilityCheckResult> CheckOperatingHoursAsync(
        int facilityId,
        DateTime start,
        DateTime end,
        CancellationToken ct)
    {
        // Pobierz wszystkie godziny otwarcia dla obiektu
        var operatingHours = await _db.OperatingHours
            .Where(o => o.FacilityId == facilityId)
            .ToListAsync(ct);

        // Jeśli nie ma zdefiniowanych godzin otwarcia, zakładamy że obiekt jest otwarty 24/7
        if (!operatingHours.Any())
        {
            return new AvailabilityCheckResult(true, AvailabilityConflictType.None, null);
        }

        // Sprawdź każdy dzień w zakresie rezerwacji
        var currentDate = start.Date;
        while (currentDate <= end.Date)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var hoursForDay = operatingHours.FirstOrDefault(o => o.DayOfWeek == dayOfWeek);

            // Jeśli nie ma wpisu dla tego dnia, zakładamy że obiekt jest zamknięty
            if (hoursForDay == null)
            {
                return new AvailabilityCheckResult(
                    false,
                    AvailabilityConflictType.FacilityClosed,
                    $"Obiekt jest zamknięty w dniu {currentDate:yyyy-MM-dd} ({GetPolishDayName(dayOfWeek)})");
            }

            // Sprawdź czy obiekt jest oznaczony jako zamknięty w tym dniu
            if (hoursForDay.IsClosed)
            {
                return new AvailabilityCheckResult(
                    false,
                    AvailabilityConflictType.FacilityClosed,
                    $"Obiekt jest zamknięty w dniu {currentDate:yyyy-MM-dd} ({GetPolishDayName(dayOfWeek)})");
            }

            // Oblicz rzeczywiste godziny rezerwacji dla tego dnia
            var dayStart = currentDate == start.Date 
                ? TimeOnly.FromDateTime(start) 
                : hoursForDay.OpenTime;
            
            var dayEnd = currentDate == end.Date 
                ? TimeOnly.FromDateTime(end) 
                : hoursForDay.CloseTime;

            // Sprawdź czy rezerwacja mieści się w godzinach otwarcia
            if (dayStart < hoursForDay.OpenTime)
            {
                return new AvailabilityCheckResult(
                    false,
                    AvailabilityConflictType.OutsideOperatingHours,
                    $"Rezerwacja rozpoczyna się przed godziną otwarcia ({hoursForDay.OpenTime}) w dniu {currentDate:yyyy-MM-dd}");
            }

            if (dayEnd > hoursForDay.CloseTime)
            {
                return new AvailabilityCheckResult(
                    false,
                    AvailabilityConflictType.OutsideOperatingHours,
                    $"Rezerwacja kończy się po godzinie zamknięcia ({hoursForDay.CloseTime}) w dniu {currentDate:yyyy-MM-dd}");
            }

            currentDate = currentDate.AddDays(1);
        }

        return new AvailabilityCheckResult(true, AvailabilityConflictType.None, null);
    }

    private async Task<AvailabilityCheckResult> CheckTimeBlocksAsync(
        int facilityId,
        DateTime start,
        DateTime end,
        CancellationToken ct)
    {
        var conflictingBlock = await _db.TimeBlocks
            .Where(t => t.FacilityId == facilityId)
            .Where(t => t.IsActive)
            .Where(t => t.StartTime < end && t.EndTime > start)
            .FirstOrDefaultAsync(ct);

        if (conflictingBlock != null)
        {
            var blockTypeName = GetPolishBlockTypeName(conflictingBlock.BlockType);
            var message = string.IsNullOrEmpty(conflictingBlock.Reason)
                ? $"Obiekt jest niedostępny z powodu: {blockTypeName} ({conflictingBlock.StartTime:yyyy-MM-dd HH:mm} - {conflictingBlock.EndTime:yyyy-MM-dd HH:mm})"
                : $"Obiekt jest niedostępny z powodu: {blockTypeName} - {conflictingBlock.Reason} ({conflictingBlock.StartTime:yyyy-MM-dd HH:mm} - {conflictingBlock.EndTime:yyyy-MM-dd HH:mm})";

            return new AvailabilityCheckResult(
                false,
                AvailabilityConflictType.TimeBlock,
                message);
        }

        return new AvailabilityCheckResult(true, AvailabilityConflictType.None, null);
    }

    private async Task<AvailabilityCheckResult> CheckExistingBookingsAsync(
        int facilityId,
        DateTime start,
        DateTime end,
        int? excludeBookingId,
        CancellationToken ct)
    {
        var query = _db.Bookings
            .Where(b => b.FacilityId == facilityId)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.Start < end && b.End > start);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        var conflictingBooking = await query.FirstOrDefaultAsync(ct);

        if (conflictingBooking != null)
        {
            return new AvailabilityCheckResult(
                false,
                AvailabilityConflictType.ExistingBooking,
                $"Obiekt jest już zarezerwowany w tym terminie ({conflictingBooking.Start:yyyy-MM-dd HH:mm} - {conflictingBooking.End:yyyy-MM-dd HH:mm})");
        }

        return new AvailabilityCheckResult(true, AvailabilityConflictType.None, null);
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

    private static string GetPolishBlockTypeName(BlockType blockType) => blockType switch
    {
        BlockType.Maintenance => "Przerwa techniczna",
        BlockType.SpecialEvent => "Wydarzenie specjalne",
        BlockType.Holiday => "Dzień wolny",
        BlockType.Other => "Inne",
        _ => blockType.ToString()
    };
}