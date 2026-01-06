using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Customers.GetCustomers;

namespace SportsCenter.API.Endpoints.CustomersEndpoints.GET;

public class GetCustomersEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(CustomersRoutes.Base,
                async (GetCustomersHandler handler, CancellationToken ct)
                    => Results.Ok(await handler.Handle(ct)))
            .WithName("GetCustomers")
            .WithTags("Customers")
            .RequireRateLimiting("per-customer");
    }
}