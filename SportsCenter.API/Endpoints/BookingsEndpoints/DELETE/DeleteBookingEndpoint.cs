using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
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
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("DeleteBooking")
            .WithTags("Bookings")
            .WithSummary("Usuwa rezerwacjê.")
            .WithDescription(
                "Usuwa istniej¹c¹ rezerwacjê na podstawie jej identyfikatora. " +
                "Je¿eli rezerwacja o podanym identyfikatorze nie istnieje, zwracany jest status 404.")
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors(); 
    }
}