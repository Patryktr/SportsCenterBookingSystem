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
                async (SearchAvailabilityRequest req, SearchAvailabilityHandler handler, HttpContext httpContext, CancellationToken ct) =>
                {
                    var result = await handler.Handle(req, ct);
                    
                    if (result.IsSuccess)
                        return Results.Ok(result.Value);

                    var error = result.Error ?? "Nie można wyszukać dostępności";
                    
                    return Results.BadRequest(new { error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("SearchAvailability")
            .WithTags("Availability")
            .WithSummary("Wyszukuje dostępne obiekty w podanym terminie.")
            .WithDescription(
                "Zwraca listę wszystkich obiektów sportowych dostępnych w podanym przedziale czasowym. " +
                "Można filtrować wyniki po typie sportu i minimalnej liczbie graczy. " +
                "Wyniki zawierają obliczoną cenę całkowitą rezerwacji dla każdego obiektu.")
            .Produces<SearchAvailabilityResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}