using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Availability.CheckAvailability;

namespace SportsCenter.API.Endpoints.AvailabilityEndpoints.CHECK;

public class CheckAvailabilityEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{AvailabilityRoutes.Base}/check",
                async (CheckAvailabilityRequest req, CheckAvailabilityHandler handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(req, ct);
                    
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(new { error = result.Error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("CheckAvailability")
            .WithTags("Availability")
            .WithSummary("Sprawdza dostępność obiektu w danym terminie.")
            .WithDescription(
                "Weryfikuje czy dany obiekt sportowy jest dostępny w podanym przedziale czasowym. " +
                "Sprawdza istniejące rezerwacje, blokady terminów oraz godziny otwarcia.")
            .Produces<CheckAvailabilityResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesStandardErrors();
    }
}