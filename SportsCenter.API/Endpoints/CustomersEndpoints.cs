using Microsoft.AspNetCore.Mvc;
using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Customers.CreateCustomer;

namespace SportsCenter.API.Endpoints;
public class CustomersEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers");

        // POST /api/customers
        group.MapPost("/", async (
            [FromBody] CreateCustomerRequest request,
            CreateCustomerHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/customers/{result.Id}", result);
        });
    }
}
