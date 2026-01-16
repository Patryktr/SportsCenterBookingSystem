using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.CreateBooking;

public class CreateBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public CreateBookingHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingRequest request, CancellationToken ct)
    {
        // ========== WALIDACJA PEŁNYCH GODZIN ==========
        if (!IsFullHour(request.Start))
            return Result<CreateBookingResponse>.Failure(
                "Rezerwacja musi rozpoczynać się o pełnej godzinie (np. 10:00, 11:00)");
        
        if (!IsFullHour(request.End))
            return Result<CreateBookingResponse>.Failure(
                "Rezerwacja musi kończyć się o pełnej godzinie (np. 10:00, 11:00)");

        // ========== WALIDACJA DAT ==========
        if (request.Start >= request.End)
            return Result<CreateBookingResponse>.Failure(
                "Data rozpoczęcia musi być wcześniejsza niż data zakończenia");

        if (request.Start < DateTime.UtcNow)
            return Result<CreateBookingResponse>.Failure(
                "Nie można zarezerwować w przeszłości");

        // ========== WALIDACJA FACILITY ==========
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        if (facility == null)
            return Result<CreateBookingResponse>.Failure("Obiekt sportowy nie istnieje");

        if (!facility.IsActive)
            return Result<CreateBookingResponse>.Failure("Obiekt sportowy jest nieaktywny");

        // ========== WALIDACJA CUSTOMERA ==========
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.PublicId == request.CustomerPublicId, ct);
        
        if (customer == null)
            return Result<CreateBookingResponse>.Failure("Klient nie istnieje");

        // ========== WALIDACJA LICZBY GRACZY ==========
        if (request.PlayersCount < 1)
            return Result<CreateBookingResponse>.Failure(
                "Liczba graczy musi być większa niż 0");

        if (request.PlayersCount > facility.MaxPlayers)
            return Result<CreateBookingResponse>.Failure(
                $"Maksymalna liczba graczy dla tego obiektu to {facility.MaxPlayers}");

        // ========== SPRAWDZENIE GODZIN OTWARCIA ==========
        var operatingHoursCheck = await CheckOperatingHoursAsync(request.FacilityId, request.Start, request.End, ct);
        if (!operatingHoursCheck.IsAvailable)
            return Result<CreateBookingResponse>.Failure(operatingHoursCheck.Message!);

        // ========== SPRAWDZENIE BLOKAD ==========
        var timeBlockCheck = await CheckTimeBlocksAsync(request.FacilityId, request.Start, request.End, ct);
        if (!timeBlockCheck.IsAvailable)
            return Result<CreateBookingResponse>.Failure(timeBlockCheck.Message!);

        // ========== SPRAWDZENIE ISTNIEJĄCYCH REZERWACJI ==========
        var hasConflict = await _db.Bookings
            .Where(b => b.FacilityId == request.FacilityId)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.Start < request.End && b.End > request.Start)
            .AnyAsync(ct);

        if (hasConflict)
            return Result<CreateBookingResponse>.Failure(
                "Ten obiekt jest już zarezerwowany w wybranym terminie");

        // ========== OBLICZENIE CENY ==========
        var duration = request.End - request.Start;
        var hours = (decimal)duration.TotalHours;
        var totalPrice = hours * facility.PricePerHour;

        // ========== UTWORZENIE REZERWACJI ==========
        var booking = new Booking
        {
            FacilityId = request.FacilityId,
            CustomerId = customer.Id,
            Start = request.Start,
            End = request.End,
            PlayersCount = request.PlayersCount,
            TotalPrice = totalPrice,
            Status = BookingStatus.Active,
            Type = request.Type
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        // Załaduj relacje dla odpowiedzi
        await _db.Entry(booking).Reference(b => b.Facility).LoadAsync(ct);
        await _db.Entry(booking).Reference(b => b.Customer).LoadAsync(ct);

        return Result<CreateBookingResponse>.Success(new CreateBookingResponse
        {
            Id = booking.Id,
            FacilityId = booking.FacilityId,
            FacilityName = booking.Facility.Name,
            CustomerPublicId = booking.Customer.PublicId,
            CustomerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}",
            Start = booking.Start,
            End = booking.End,
            PlayersCount = booking.PlayersCount,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            Type = booking.Type
        });
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

        // Jeśli nie ma zdefiniowanych godzin otwarcia, zakładamy że obiekt jest otwarty 24/7
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
                ? $"Obiekt jest niedostępny z powodu: {blockTypeName} ({conflictingBlock.StartTime:yyyy-MM-dd HH:mm} - {conflictingBlock.EndTime:yyyy-MM-dd HH:mm})"
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