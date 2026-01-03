using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Facilities.CreateFacility;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.CREATE;
public class CreateFacilityEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{FacilitiesRoutes.Base}",
                async (CreateFacilityRequest req, CreateFacilityHandler h, CancellationToken ct)
                    => Results.Ok(await h.Handle(req,ct)))
            .WithName("CreateFacility")
            .WithTags("Facilities");
    }
}