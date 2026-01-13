using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Application.Common;
using SportsCenter.Infrastructure.Persistence;
using OperatingHoursEntity = SportsCenter.Domain.Entities.OperatingHours;

namespace SportsCenter.Application.Features.OperatingHours.SetOperatingHours;

public class SetOperatingHoursHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public SetOperatingHoursHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SetOperatingHoursResponse>> Handle(SetOperatingHoursRequest request, CancellationToken ct = default)
    {
        // Sprawdź czy obiekt istnieje
        var facility = await _db.Facilities.FindAsync(new object[] { request.FacilityId }, ct);
        if (facility == null)
        {
            return Result<SetOperatingHoursResponse>.Failure("Obiekt sportowy nie istnieje");
        }

        // Walidacja danych wejściowych
        foreach (var day in request.Schedule)
        {
            if (!day.IsClosed)
            {
                if (string.IsNullOrEmpty(day.OpenTime) || string.IsNullOrEmpty(day.CloseTime))
                {
                    return Result<SetOperatingHoursResponse>.Failure(
                        $"Dla dnia {GetPolishDayName(day.DayOfWeek)} należy podać godziny otwarcia i zamknięcia lub oznaczyć jako zamknięty");
                }

                if (!TimeOnly.TryParse(day.OpenTime, out var openTime) || 
                    !TimeOnly.TryParse(day.CloseTime, out var closeTime))
                {
                    return Result<SetOperatingHoursResponse>.Failure(
                        $"Nieprawidłowy format godziny dla dnia {GetPolishDayName(day.DayOfWeek)}. Użyj formatu HH:mm");
                }

                if (openTime >= closeTime)
                {
                    return Result<SetOperatingHoursResponse>.Failure(
                        $"Godzina otwarcia musi być wcześniejsza niż godzina zamknięcia dla dnia {GetPolishDayName(day.DayOfWeek)}");
                }
            }
        }

        // Usuń istniejące godziny otwarcia dla tego obiektu
        var existingHours = await _db.OperatingHours
            .Where(o => o.FacilityId == request.FacilityId)
            .ToListAsync(ct);
        
        _db.OperatingHours.RemoveRange(existingHours);

        // Dodaj nowe godziny otwarcia
        var newHours = new List<OperatingHoursEntity>();
        foreach (var day in request.Schedule)
        {
            var operatingHours = new OperatingHoursEntity
            {
                FacilityId = request.FacilityId,
                DayOfWeek = day.DayOfWeek,
                IsClosed = day.IsClosed,
                OpenTime = day.IsClosed ? TimeOnly.MinValue : TimeOnly.Parse(day.OpenTime!),
                CloseTime = day.IsClosed ? TimeOnly.MinValue : TimeOnly.Parse(day.CloseTime!)
            };
            newHours.Add(operatingHours);
        }

        _db.OperatingHours.AddRange(newHours);
        await _db.SaveChangesAsync(ct);

        // Przygotuj odpowiedź
        var response = new SetOperatingHoursResponse
        {
            FacilityId = facility.Id,
            FacilityName = facility.Name,
            Schedule = newHours
                .OrderBy(h => h.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)h.DayOfWeek)
                .Select(h => new OperatingHoursItem
                {
                    DayOfWeek = h.DayOfWeek,
                    DayName = GetPolishDayName(h.DayOfWeek),
                    OpenTime = h.IsClosed ? null : h.OpenTime.ToString("HH:mm"),
                    CloseTime = h.IsClosed ? null : h.CloseTime.ToString("HH:mm"),
                    IsClosed = h.IsClosed
                })
                .ToList()
        };

        return Result<SetOperatingHoursResponse>.Success(response);
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