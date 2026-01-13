using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Facilities.DeleteFacility;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.DELETE;

public class DeleteFacilityEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{FacilitiesRoutes.Base}/{{id:int}}",
                async (int id, DeleteFacilityHandler h, CancellationToken ct)
                    => await h.Handle(id, ct)
                        ? Results.Ok()
                        : Results.NotFound())
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("DeleteFacility")
            .WithTags("Facilities")
            .WithSummary("Usuwa obiekt sportowy.")
            .WithDescription(
                "Usuwa obiekt sportowy na podstawie jego identyfikatora. " +
                "Jeżeli obiekt o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors();
    }
}