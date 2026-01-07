using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Bookings.DeleteBooking;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.DELETE;

public class DeleteBookingEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{BookingsRoutes.Base}/{{id:int}}",
                async (int id, DeleteBookingHandler handler, CancellationToken ct)
                    => await handler.Handle(id, ct)
                        ? Results.Ok()
                        : Results.NotFound())
            .WithName("DeleteBooking")
            .WithTags("Bookings");
    }
}