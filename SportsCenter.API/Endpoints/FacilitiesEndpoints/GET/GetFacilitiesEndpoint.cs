using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Facilities.GetFacilities;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.GET;

public class GetFacilitiesEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{FacilitiesRoutes.Base}",
                async (GetFacilitiesHandler h, CancellationToken ct)
                    => Results.Ok(await h.Handle(ct)))
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("GetFacilities")
            .WithTags("Facilities")
            .WithSummary("Zwraca listę obiektów sportowych.")
            .WithDescription(
                "Zwraca listę wszystkich obiektów sportowych dostępnych w systemie. " +
                "Każdy obiekt zawiera podstawowe informacje, takie jak typ sportu, limit graczy, cena za godzinę oraz status aktywności.")
            .Produces<IEnumerable<GetFacilitiesResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors()
            .RequireRateLimiting("per-customer");
    }
}