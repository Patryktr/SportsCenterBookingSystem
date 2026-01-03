using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Facilities.UpdateFacility;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.UPDATE;

public class UpdateFacilityEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut($"{FacilitiesRoutes.Base}/{{id:int}}",
                async (int id, UpdateFacilityRequest req, UpdateFacilityHandler h, CancellationToken ct)
                    => id != req.Id
                        ? Results.BadRequest()
                        : await h.Handle(req, ct) ? Results.Ok() : Results.NotFound())
            .WithName("UpdateFacility")
            .WithTags("Facilities");
    }
}