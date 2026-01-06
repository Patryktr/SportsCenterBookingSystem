using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Abstractions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Application.Features.Customers.GetCustomerById;

public class GetCustomerByIdHandler : IHandlerDefinition
{
    private readonly SportsCenterDbContext _db;

    public GetCustomerByIdHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<GetCustomerByIdResponse?> Handle(Guid publicId, CancellationToken ct = default)
    {
        return await _db.Customers
            .Where(c => c.PublicId == publicId)
            .Select(c => new GetCustomerByIdResponse
            {
                PublicId = c.PublicId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone
            })
            .SingleOrDefaultAsync(ct);
    }
}