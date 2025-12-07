using SportsCenter.Domain.Entities;
using SportsCenter.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SportsCenter.Application.Features.Customers.CreateCustomer;
public class CreateCustomerHandler
{
    private readonly SportsCenterDbContext _db;

    public CreateCustomerHandler(SportsCenterDbContext db)
    {
        _db = db;
    }

    public async Task<CreateCustomerResponse> HandleAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name is required");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        var emailExists = await _db.Customers
            .AnyAsync(c => c.Email == request.Email, cancellationToken);

        if (emailExists)
            throw new InvalidOperationException("Customer with this email already exists");

        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateCustomerResponse
        {
            Id = customer.PublicId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone
        };
    }
}
