using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Customers.CreateCustomer;

namespace SportsCenter.API.Endpoints.CustomersEndpoints.CREATE;

public class CreateCustomerEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(CustomersRoutes.Base,
                async (CreateCustomerRequest req, CreateCustomerHandler handler, CancellationToken ct)
                    => Results.Ok(await handler.Handle(req, ct)))
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("CreateCustomer")
            .WithTags("Customers")
            .WithSummary("Tworzy nowego klienta.")
            .WithDescription(
                "Tworzy nowego klienta na podstawie przekazanych danych kontaktowych. " +
                "W przypadku powodzenia zwracany jest publiczny identyfikator klienta, " +
                "który może być wykorzystywany w dalszych operacjach, np. przy tworzeniu rezerwacji.")
            .Produces<CreateCustomerResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}