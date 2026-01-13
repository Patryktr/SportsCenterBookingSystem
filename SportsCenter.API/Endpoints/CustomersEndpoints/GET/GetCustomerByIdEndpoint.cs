using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Customers.GetCustomerById;

namespace SportsCenter.API.Endpoints.CustomersEndpoints.GET;

public class GetCustomerByIdEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{CustomersRoutes.Base}/{{publicId:guid}}",
                async (Guid publicId, GetCustomerByIdHandler handler, CancellationToken ct)
                    => (await handler.Handle(publicId, ct)) is { } c
                        ? Results.Ok(c)
                        : Results.NotFound())
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("GetCustomerByPublicId")
            .WithTags("Customers")
            .WithSummary("Zwraca dane klienta.")
            .WithDescription(
                "Zwraca szczegółowe dane klienta na podstawie jego publicznego identyfikatora (GUID). " +
                "Jeżeli klient o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces<GetCustomerByIdResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors()
            .RequireRateLimiting("per-customer");
    }
}