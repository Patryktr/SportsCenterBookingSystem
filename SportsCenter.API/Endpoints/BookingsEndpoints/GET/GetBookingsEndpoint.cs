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
            .WithTags("Bookings");
    }
}