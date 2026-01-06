using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Bookings.CREATE;
using SportsCenter.Application.Features.Bookings.CreateBooking;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.CREATE;

public class CreateBookingEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{BookingsRoutes.Base}",
                async (CreateBookingRequest req, CreateBookingHandler h, CancellationToken ct)
                    => Results.Ok(await h.Handle(req, ct)))
            .WithName("CreateBooking")
            .WithTags("Bookings");
    }
}