using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.GetBookings;

public class GetBookingsHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public GetBookingsHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<GetBookingsResponse>> Handle(CancellationToken ct)
    {
        return await _db.Bookings
            .Include(b => b.Facility)
            .Include(b => b.Customer)
            .OrderByDescending(b => b.Start)
            .Select(b => new GetBookingsResponse
            {
                Id = b.Id,
                FacilityId = b.FacilityId,
                FacilityName = b.Facility.Name,
                SportType = b.Facility.SportType,
                CustomerPublicId = b.Customer.PublicId,
                CustomerName = $"{b.Customer.FirstName} {b.Customer.LastName}",
                Start = b.Start,
                End = b.End,
                PlayersCount = b.PlayersCount,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                Type = b.Type
            })
            .ToListAsync(ct);
    }
}