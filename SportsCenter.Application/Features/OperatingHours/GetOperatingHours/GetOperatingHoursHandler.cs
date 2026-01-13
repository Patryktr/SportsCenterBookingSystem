using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;
using SportsCenter.Application.Common;

namespace SportsCenter.Application.Features.OperatingHours.GetOperatingHours;

public class GetOperatingHoursHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public GetOperatingHoursHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<GetOperatingHoursResponse?> Handle(int facilityId, CancellationToken ct = default)
    {
        var facility = await _db.Facilities.FindAsync(new object[] { facilityId }, ct);
        if (facility == null)
        {
            return null;
        }

        var operatingHours = await _db.OperatingHours
            .Where(o => o.FacilityId == facilityId)
            .ToListAsync(ct);

        return new GetOperatingHoursResponse
        {
            FacilityId = facility.Id,
            FacilityName = facility.Name,
            Schedule = operatingHours
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