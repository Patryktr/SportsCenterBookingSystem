using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.UpdateBooking;

public class UpdateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public UpdateBookingHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(int id, UpdateBookingRequest request, CancellationToken ct = default)
    {
        // ========== WALIDACJA PEŁNYCH GODZIN ==========
        if (!IsFullHour(request.Start))
            return Result<bool>.Failure(
                "Rezerwacja musi rozpoczynać się o pełnej godzinie (np. 10:00, 11:00)");
        
        if (!IsFullHour(request.End))
            return Result<bool>.Failure(
                "Rezerwacja musi kończyć się o pełnej godzinie (np. 10:00, 11:00)");

        // ========== POBRANIE REZERWACJI ==========
        var booking = await _db.Bookings
            .Include(b => b.Facility)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking == null)
            return Result<bool>.Failure("Rezerwacja nie istnieje");

        if (booking.Status == BookingStatus.Canceled)
            return Result<bool>.Failure("Nie można edytować anulowanej rezerwacji");

        // ========== WALIDACJA DAT ==========
        if (request.Start >= request.End)
            return Result<bool>.Failure(
                "Data rozpoczęcia musi być wcześniejsza niż data zakończenia");

        if (request.Start < DateTime.UtcNow)
            return Result<bool>.Failure(
                "Nie można zarezerwować w przeszłości");

        // ========== WALIDACJA LICZBY GRACZY ==========
        if (request.PlayersCount < 1)
            return Result<bool>.Failure(
                "Liczba graczy musi być większa niż 0");

        if (request.PlayersCount > booking.Facility.MaxPlayers)
            return Result<bool>.Failure(
                $"Maksymalna liczba graczy dla tego obiektu to {booking.Facility.MaxPlayers}");

        // ========== SPRAWDZENIE GODZIN OTWARCIA ==========
        var operatingHoursCheck = await CheckOperatingHoursAsync(booking.FacilityId, request.Start, request.End, ct);
        if (!operatingHoursCheck.IsAvailable)
            return Result<bool>.Failure(operatingHoursCheck.Message!);

        // ========== SPRAWDZENIE BLOKAD ==========
        var timeBlockCheck = await CheckTimeBlocksAsync(booking.FacilityId, request.Start, request.End, ct);
        if (!timeBlockCheck.IsAvailable)
            return Result<bool>.Failure(timeBlockCheck.Message!);

        // ========== SPRAWDZENIE ISTNIEJĄCYCH REZERWACJI (z wykluczeniem tej) ==========
        var hasConflict = await _db.Bookings
            .Where(b => b.FacilityId == booking.FacilityId)
            .Where(b => b.Id != id)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.Start < request.End && b.End > request.Start)
            .AnyAsync(ct);

        if (hasConflict)
            return Result<bool>.Failure(
                "Ten obiekt jest już zarezerwowany w wybranym terminie");

        // ========== AKTUALIZACJA REZERWACJI ==========
        var duration = request.End - request.Start;
        var hours = (decimal)duration.TotalHours;
        var totalPrice = hours * booking.Facility.PricePerHour;

        booking.Start = request.Start;
        booking.End = request.End;
        booking.PlayersCount = request.PlayersCount;
        booking.TotalPrice = totalPrice;

        await _db.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ==================== METODY POMOCNICZE ====================

    private static bool IsFullHour(DateTime dateTime)
    {
        return dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
    }

    private async Task<(bool IsAvailable, string? Message)> CheckOperatingHoursAsync(
        int facilityId, DateTime start, DateTime end, CancellationToken ct)
    {
        var operatingHours = await _db.OperatingHours
            .Where(o => o.FacilityId == facilityId)
            .ToListAsync(ct);

        if (!operatingHours.Any())
            return (true, null);

        var currentDate = start.Date;
        while (currentDate <= end.Date)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var hoursForDay = operatingHours.FirstOrDefault(o => o.DayOfWeek == dayOfWeek);

            if (hoursForDay == null || hoursForDay.IsClosed)
            {
                return (false, $"Obiekt jest zamknięty w dniu {currentDate:yyyy-MM-dd} ({GetPolishDayName(dayOfWeek)})");
            }

            var dayStart = currentDate == start.Date 
                ? TimeOnly.FromDateTime(start) 
                : hoursForDay.OpenTime;
            
            var dayEnd = currentDate == end.Date 
                ? TimeOnly.FromDateTime(end) 
                : hoursForDay.CloseTime;

            if (dayStart < hoursForDay.OpenTime)
            {
                return (false, $"Rezerwacja rozpoczyna się przed godziną otwarcia ({hoursForDay.OpenTime}) w dniu {currentDate:yyyy-MM-dd}");
            }

            if (dayEnd > hoursForDay.CloseTime)
            {
                return (false, $"Rezerwacja kończy się po godzinie zamknięcia ({hoursForDay.CloseTime}) w dniu {currentDate:yyyy-MM-dd}");
            }

            currentDate = currentDate.AddDays(1);
        }

        return (true, null);
    }

    private async Task<(bool IsAvailable, string? Message)> CheckTimeBlocksAsync(
        int facilityId, DateTime start, DateTime end, CancellationToken ct)
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
                ? $"Obiekt jest niedostępny z powodu: {blockTypeName}"
                : $"Obiekt jest niedostępny: {blockTypeName} - {conflictingBlock.Reason}";

            return (false, message);
        }

        return (true, null);
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