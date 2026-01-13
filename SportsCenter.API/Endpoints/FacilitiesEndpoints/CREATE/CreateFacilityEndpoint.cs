using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Facilities.CreateFacility;

namespace SportsCenter.API.Endpoints.FacilitiesEndpoints.CREATE;
public class CreateFacilityEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{FacilitiesRoutes.Base}",
                async (CreateFacilityRequest req, CreateFacilityHandler h, CancellationToken ct)
                    => Results.Ok(await h.Handle(req,ct)))
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("CreateFacility")
            .WithTags("Facilities")
            .WithSummary("Tworzy nowy obiekt sportowy.")
            .WithDescription(
                "Tworzy nowy obiekt sportowy (np. kort/boisko) na podstawie przekazanych danych. " +
                "Nazwa obiektu powinna być unikalna. W przypadku powodzenia zwracane są dane utworzonego obiektu.")
            .Produces<CreateFacilityResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}