using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Bookings.GetBookings;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.GET;

public class GetBookingsEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(BookingsRoutes.Base,
                async (GetBookingsHandler handler, CancellationToken ct)
                    => Results.Ok(await handler.Handle(ct)))
            .WithName("GetBookings")
            .WithTags("Bookings")
            .WithSummary("Zwraca listê rezerwacji.")
            .WithDescription(
                "Zwraca listê wszystkich rezerwacji obiektów sportowych. " +
                "Ka¿da pozycja zawiera podstawowe informacje o rezerwacji, takie jak obiekt, klient, " +
                "termin oraz status rezerwacji.")
            .Produces<IEnumerable<GetBookingsResponse>>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors()
            .RequireRateLimiting("per-customer");
    }
}