using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.TimeBlocks.DeleteTimeBlock;

namespace SportsCenter.API.Endpoints.TimeBlocksEndpoints.DELETE;

public class DeleteTimeBlockEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{TimeBlocksRoutes.Base}/{{id:int}}",
                async (int id, DeleteTimeBlockHandler handler, CancellationToken ct)
                    => await handler.Handle(id, ct)
                        ? Results.Ok()
                        : Results.NotFound())
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("DeleteTimeBlock")
            .WithTags("Time Blocks")
            .WithSummary("Usuwa blokadę terminu.")
            .WithDescription(
                "Usuwa blokadę terminu na podstawie jej identyfikatora. " +
                "Jeżeli blokada o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors();
    }
}