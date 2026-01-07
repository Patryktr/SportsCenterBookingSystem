using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Bookings.GetBookingById;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.GET;

public class GetBookingByIdEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"{BookingsRoutes.Base}/{{id:int}}",
                async (int id, GetBookingByIdHandler handler, CancellationToken ct)
                    => await handler.Handle(id, ct) is { } booking
                        ? Results.Ok(booking)
                        : Results.NotFound())
            .WithName("GetBookingById")
            .WithTags("Bookings")
            .WithSummary("Zwraca szczegó³y rezerwacji.")
            .WithDescription(
                "Zwraca szczegó³owe informacje o rezerwacji na podstawie jej identyfikatora. " +
                "Je¿eli rezerwacja o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces<GetBookingByIdResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors()
            .RequireRateLimiting("per-customer");
    }
}