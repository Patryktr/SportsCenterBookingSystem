using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Customers.DeleteCustomer;

namespace SportsCenter.API.Endpoints.CustomersEndpoints.DELETE;

public class DeleteCustomerEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{CustomersRoutes.Base}/{{publicId:guid}}",
                async (Guid publicId, DeleteCustomerHandler handler, CancellationToken ct)
                    => await handler.Handle(publicId, ct)
                        ? Results.Ok()
                        : Results.NotFound())
            .WithName("DeleteCustomer")
            .WithTags("Customers");
    }
}