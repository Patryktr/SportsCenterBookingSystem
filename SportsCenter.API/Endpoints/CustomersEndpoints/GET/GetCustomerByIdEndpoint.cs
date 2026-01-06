using SportsCenter.API.Extentions;
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
            .WithName("GetCustomerByPublicId")
            .WithTags("Customers")
            .RequireRateLimiting("per-customer");
    }
}