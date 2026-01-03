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
            .WithTags("Facilities");
    }
}