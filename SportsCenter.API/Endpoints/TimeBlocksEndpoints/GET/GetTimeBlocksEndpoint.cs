using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.TimeBlocks.GetTimeBlocks;

namespace SportsCenter.API.Endpoints.TimeBlocksEndpoints.GET;

public class GetTimeBlocksEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(TimeBlocksRoutes.Base,
                async (int? facilityId, bool? activeOnly, GetTimeBlocksHandler handler, CancellationToken ct)
                    => Results.Ok(await handler.Handle(facilityId, activeOnly, ct)))
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("GetTimeBlocks")
            .WithTags("Time Blocks")
            .WithSummary("Pobiera listę blokad terminów.")
            .WithDescription(
                "Zwraca listę blokad terminów. Można filtrować po obiekcie sportowym (facilityId) " +
                "oraz statusie aktywności (activeOnly=true zwraca tylko aktywne blokady).")
            .Produces<IEnumerable<GetTimeBlocksResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}