using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.OperatingHours.GetOperatingHours;

namespace SportsCenter.API.Endpoints.OperatingHoursEndpoints.GET;

public class GetOperatingHoursEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(OperatingHoursRoutes.Base,
                async (int facilityId, GetOperatingHoursHandler handler, CancellationToken ct)
                    => await handler.Handle(facilityId, ct) is { } result
                        ? Results.Ok(result)
                        : Results.NotFound())
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("GetOperatingHours")
            .WithTags("Operating Hours")
            .WithSummary("Pobiera godziny otwarcia obiektu.")
            .WithDescription(
                "Zwraca harmonogram godzin otwarcia dla wskazanego obiektu sportowego. " +
                "Zawiera informacje o godzinach otwarcia i zamknięcia dla każdego dnia tygodnia.")
            .Produces<GetOperatingHoursResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}