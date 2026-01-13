using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Customers.GetCustomers;

namespace SportsCenter.API.Endpoints.CustomersEndpoints.GET;

public class GetCustomersEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(CustomersRoutes.Base,
                async (GetCustomersHandler handler, CancellationToken ct)
                    => Results.Ok(await handler.Handle(ct)))
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("GetCustomers")
            .WithTags("Customers")
            .WithSummary("Zwraca listę klientów.")
            .WithDescription(
                "Zwraca listę wszystkich klientów zapisanych w systemie. " +
                "Każda pozycja zawiera podstawowe dane identyfikacyjne oraz kontaktowe klienta.")
            .Produces<IEnumerable<GetCustomersResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors()
            .RequireRateLimiting("per-customer");
    }
}