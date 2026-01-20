using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Availability.SearchAvailability;

namespace SportsCenter.API.Endpoints.AvailabilityEndpoints.SEARCH;

public class SearchAvailabilityEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{AvailabilityRoutes.Base}/search",
                async (SearchAvailabilityRequest req, SearchAvailabilityHandler handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(req, ct);
                    
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(new { error = result.Error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("SearchAvailability")
            .WithTags("Availability")
            .WithSummary("Wyszukuje dostępne sloty godzinowe dla obiektu.")
            .WithDescription(
                "Zwraca listę slotów godzinowych dla wybranego obiektu sportowego w danym dniu. " +
                "Każdy slot ma status: Wolne, Zarezerwowane, Zablokowane, Zamknięte lub Minione. " +
                "Rezerwacje można tworzyć tylko na pełne godziny (np. 10:00-11:00, 14:00-16:00).")
            .Produces<SearchAvailabilityResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesStandardErrors();
    }
}