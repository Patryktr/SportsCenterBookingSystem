using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Facilities.GetFacilities;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.GET;

public class GetFacilitiesEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{FacilitiesRoutes.Base}",
                async (GetFacilitiesHandler h, CancellationToken ct)
                    => Results.Ok(await h.Handle(ct)))
            .WithName("GetFacilities")
            .WithTags("Facilities");
    }
}