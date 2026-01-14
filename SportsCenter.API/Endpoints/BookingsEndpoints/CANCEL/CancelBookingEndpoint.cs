using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Bookings.CancelBooking;
using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.CANCEL;

public class CancelBookingEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{BookingsRoutes.Base}/{{id:int}}/cancel",
                async (int id, CancelBookingHandler handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(id, ct);
                    
                    return result.Result switch
                    {
                        CancellationResult.Success => Results.Ok(result),
                        CancellationResult.AlreadyCancelled => Results.NoContent(),
                        CancellationResult.NotFound => Results.NotFound(new { error = result.Message }),
                        CancellationResult.TooLateToCancel => Results.Conflict(new { error = result.Message }),
                        _ => Results.BadRequest(new { error = "Nieznany błąd" })
                    };
                })
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("CancelBooking")
            .WithTags("Bookings")
            .WithSummary("Anuluje rezerwację.")
            .WithDescription(
                "Anuluje rezerwację najpóźniej na 1 godzinę przed rozpoczęciem. " +
                "Operacja jest idempotentna - powtórne anulowanie zwraca 204 No Content.")
            .Produces<CancelBookingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesStandardErrors();
    }
}