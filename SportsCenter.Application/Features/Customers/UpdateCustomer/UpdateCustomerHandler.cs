using SportsCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;

namespace SportsCenter.Application.Features.Customers.UpdateCustomer;

public class UpdateCustomerHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public UpdateCustomerHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(Guid publicId, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _db.Customers
            .SingleOrDefaultAsync(c => c.PublicId == publicId, ct);

        if (customer is null)
            return false;

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Phone = request.Phone;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}