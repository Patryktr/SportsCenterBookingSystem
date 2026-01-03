using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Customers.CreateCustomer;

namespace SportsCenter.API.Endpoints.CustomersEndpoints.CREATE;

public class CreateCustomerEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(CustomersRoutes.Base,
                async (CreateCustomerRequest req, CreateCustomerHandler handler, CancellationToken ct)
                    => Results.Ok(await handler.Handle(req, ct)))
            .WithName("CreateCustomer")
            .WithTags("Customers");
    }
}