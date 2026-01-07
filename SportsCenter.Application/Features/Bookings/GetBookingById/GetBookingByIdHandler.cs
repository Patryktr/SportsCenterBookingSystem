using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.GetBookingById;

public class GetBookingByIdHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public GetBookingByIdHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<GetBookingByIdResponse?> Handle(int id, CancellationToken ct)
    {
        return await _db.Bookings
            .Include(b => b.Facility)
            .Include(b => b.Customer)
            .Where(b => b.Id == id)
            .Select(b => new GetBookingByIdResponse
            {
                Id = b.Id,
                FacilityId = b.FacilityId,
                FacilityName = b.Facility.Name,
                SportType = b.Facility.SportType,
                FacilityPricePerHour = b.Facility.PricePerHour,
                CustomerPublicId = b.Customer.PublicId,
                CustomerFirstName = b.Customer.FirstName,
                CustomerLastName = b.Customer.LastName,
                CustomerEmail = b.Customer.Email,
                CustomerPhone = b.Customer.Phone,
                Start = b.Start,
                End = b.End,
                PlayersCount = b.PlayersCount,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                Type = b.Type
            })
            .FirstOrDefaultAsync();
    }
}