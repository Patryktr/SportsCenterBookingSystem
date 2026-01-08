using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Facilities.GetFacilityById;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.GET;

public class GetFacilityByIdEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{FacilitiesRoutes.Base}/{{id:int}}",
                async (int id, GetFacilityByIdHandler h, CancellationToken ct)
                    => await h.Handle(id, ct) is { } f
                        ? Results.Ok(f)
                        : Results.NotFound())
            .WithName("GetFacilityById")
            .WithTags("Facilities")
            .WithSummary("Zwraca szczegóły obiektu sportowego.")
            .WithDescription(
                "Zwraca szczegółowe informacje o obiekcie sportowym na podstawie jego identyfikatora. " +
                "Jeżeli obiekt o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces<GetFacilityByIdResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors()
            .RequireRateLimiting("per-customer");
    }
}