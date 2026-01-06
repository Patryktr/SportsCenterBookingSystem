using SportsCenter.API.Extentions;
using SportsCenter.Application.Features.Bookings.UpdateBooking;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.UPDATE;

public class UpdateBookingEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut($"{BookingsRoutes.Base}/{{id:int}}",
                async (int id, UpdateBookingRequest req, UpdateBookingHandler handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(id, req, ct);
                    
                    return result.IsSuccess
                        ? Results.Ok()
                        : Results.BadRequest(new { error = result.Error });
                })
            .WithName("UpdateBooking")
            .WithTags("Bookings");
    }
}