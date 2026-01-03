using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Customers.DeleteCustomer;

public class DeleteCustomerHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public DeleteCustomerHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(Guid publicId, CancellationToken ct = default)
    {
        var customer = await _db.Customers
            .SingleOrDefaultAsync(c => c.PublicId == publicId, ct);

        if (customer is null)
            return false;

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}