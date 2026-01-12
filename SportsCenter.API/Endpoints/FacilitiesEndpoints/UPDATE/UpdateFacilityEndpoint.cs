using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
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
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("UpdateFacility")
            .WithTags("Facilities")
            .WithSummary("Aktualizuje obiekt sportowy.")
            .WithDescription(
                "Aktualizuje dane istniejącego obiektu sportowego na podstawie identyfikatora w URL. " +
                "Identyfikator w URL musi być zgodny z identyfikatorem w body. " +
                "Jeżeli obiekt o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors();
    }
}