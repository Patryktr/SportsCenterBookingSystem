using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Bookings.DeleteBooking;

public class DeleteBookingHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public DeleteBookingHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(int id, CancellationToken ct)
    {
        var booking = await _db.Bookings.FindAsync(id, ct);

        if (booking == null)
            return false;

        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();

        return true;
    }
}