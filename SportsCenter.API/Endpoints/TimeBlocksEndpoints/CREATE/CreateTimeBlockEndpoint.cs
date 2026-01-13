using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.TimeBlocks.CreateTimeBlock;

namespace SportsCenter.API.Endpoints.TimeBlocksEndpoints.CREATE;

public class CreateTimeBlockEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(TimeBlocksRoutes.Base,
                async (CreateTimeBlockRequest req, CreateTimeBlockHandler handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(req, ct);
                    
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.Conflict(new { error = result.Error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("CreateTimeBlock")
            .WithTags("Time Blocks")
            .WithSummary("Tworzy blokadę terminu.")
            .WithDescription(
                "Tworzy nową blokadę terminu (przerwa techniczna, dzień wolny, itp.) dla wskazanego obiektu sportowego. " +
                "W tym czasie obiekt nie będzie dostępny do rezerwacji. " +
                "Zwraca kod 409 Conflict jeśli istnieje już blokada w podanym przedziale czasowym.")
            .Produces<CreateTimeBlockResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status409Conflict)
            .ProducesStandardErrors();
    }
}