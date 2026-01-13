using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.OperatingHours.SetOperatingHours;

namespace SportsCenter.API.Endpoints.OperatingHoursEndpoints.POST;

public class SetOperatingHoursEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(OperatingHoursRoutes.Base,
                async (int facilityId, SetOperatingHoursRequest req, SetOperatingHoursHandler handler, CancellationToken ct) =>
                {
                    req.FacilityId = facilityId;
                    var result = await handler.Handle(req, ct);
                    
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(new { error = result.Error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("SetOperatingHours")
            .WithTags("Operating Hours")
            .WithSummary("Ustawia godziny otwarcia obiektu.")
            .WithDescription(
                "Definiuje harmonogram godzin otwarcia dla wskazanego obiektu sportowego. " +
                "Zastępuje istniejący harmonogram nowym. " +
                "Każdy dzień tygodnia może mieć własne godziny otwarcia lub być oznaczony jako zamknięty.")
            .Produces<SetOperatingHoursResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}