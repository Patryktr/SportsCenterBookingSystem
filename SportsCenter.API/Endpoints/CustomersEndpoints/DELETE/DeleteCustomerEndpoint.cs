using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
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
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("DeleteCustomer")
            .WithTags("Customers")
            .WithSummary("Usuwa klienta.")
            .WithDescription(
                "Usuwa klienta na podstawie jego publicznego identyfikatora (GUID). " +
                "Jeżeli klient o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors();
    }
}