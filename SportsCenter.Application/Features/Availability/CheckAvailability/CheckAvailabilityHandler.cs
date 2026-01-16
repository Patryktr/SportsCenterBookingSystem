using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Availability.CheckAvailability;

public class CheckAvailabilityHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public CheckAvailabilityHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CheckAvailabilityResponse>> Handle(CheckAvailabilityRequest request, CancellationToken ct = default)
    {
        // ========== WALIDACJA FACILITY ==========
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        
        if (facility == null)
        {
            return Result<CheckAvailabilityResponse>.Success(new CheckAvailabilityResponse
            {
                IsAvailable = false,
                FacilityId = request.FacilityId,
                Start = request.Start,
                End = request.End,
                ConflictReason = "Obiekt sportowy nie istnieje"
            });
        }

        if (!facility.IsActive)
        {
            return Result<CheckAvailabilityResponse>.Success(new CheckAvailabilityResponse
            {
                IsAvailable = false,
                FacilityId = request.FacilityId,
                Start = request.Start,
                End = request.End,
                ConflictReason = "Obiekt sportowy jest nieaktywny"
            });
        }

        // ========== SPRAWDZENIE GODZIN OTWARCIA ==========
        var operatingHoursCheck = await CheckOperatingHoursAsync(request.FacilityId, request.Start, request.End, ct);
        if (!operatingHoursCheck.IsAvailable)
        {
            return Result<CheckAvailabilityResponse>.Success(new CheckAvailabilityResponse
            {
                IsAvailable = false,
                FacilityId = request.FacilityId,
                Start = request.Start,
                End = request.End,
                ConflictReason = operatingHoursCheck.Message
            });
        }

        // ========== SPRAWDZENIE BLOKAD ==========
        var timeBlockCheck = await CheckTimeBlocksAsync(request.FacilityId, request.Start, request.End, ct);
        if (!timeBlockCheck.IsAvailable)
        {
            return Result<CheckAvailabilityResponse>.Success(new CheckAvailabilityResponse
            {
                IsAvailable = false,
                FacilityId = request.FacilityId,
                Start = request.Start,
                End = request.End,
                ConflictReason = timeBlockCheck.Message
            });
        }

        // ========== SPRAWDZENIE ISTNIEJĄCYCH REZERWACJI ==========
        var conflictingBooking = await _db.Bookings
            .Where(b => b.FacilityId == request.FacilityId)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.Start < request.End && b.End > request.Start)
            .FirstOrDefaultAsync(ct);

        if (conflictingBooking != null)
        {
            return Result<CheckAvailabilityResponse>.Success(new CheckAvailabilityResponse
            {
                IsAvailable = false,
                FacilityId = request.FacilityId,
                Start = request.Start,
                End = request.End,
                ConflictReason = $"Obiekt jest już zarezerwowany w tym terminie ({conflictingBooking.Start:HH:mm} - {conflictingBooking.End:HH:mm})"
            });
        }

        // ========== DOSTĘPNY ==========
        return Result<CheckAvailabilityResponse>.Success(new CheckAvailabilityResponse
        {
            IsAvailable = true,
            FacilityId = request.FacilityId,
            Start = request.Start,
            End = request.End,
            ConflictReason = null
        });
    }

    // ==================== METODY POMOCNICZE ====================

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
                return (false, $"Rezerwacja rozpoczyna się przed godziną otwarcia ({hoursForDay.OpenTime})");
            }

            if (dayEnd > hoursForDay.CloseTime)
            {
                return (false, $"Rezerwacja kończy się po godzinie zamknięcia ({hoursForDay.CloseTime})");
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