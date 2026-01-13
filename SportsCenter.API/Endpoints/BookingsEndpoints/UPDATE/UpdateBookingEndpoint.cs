using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
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
                    
                    if (result.IsSuccess)
                        return Results.Ok();
                    
                    var error = result.Error ?? "Nie można zaktualizować rezerwacji";
                    
                    if (error.Contains("nie istnieje"))
                        return Results.NotFound(new { error });
                    
                    if (IsAvailabilityConflict(error))
                        return Results.Conflict(new { error });
                    
                    return Results.BadRequest(new { error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.Admin))
            .WithName("UpdateBooking")
            .WithTags("Bookings")
            .WithSummary("Aktualizuje rezerwację.")
            .WithDescription(
                "Aktualizuje dane istniejącej rezerwacji na podstawie identyfikatora w URL. " +
                "Weryfikuje poprawność danych wejściowych, godziny otwarcia, blokady terminów oraz dostępność. " +
                "W przypadku konfliktu dostępności zwraca status 409 Conflict.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesStandardErrors();
    }

    private static bool IsAvailabilityConflict(string error)
    {
        return error.Contains("zarezerwowany") ||
               error.Contains("niedostępny") ||
               error.Contains("zamknięty") ||
               error.Contains("nieaktywny") ||
               error.Contains("otwarcia") ||
               error.Contains("zamknięcia") ||
               error.Contains("przerwa") ||
               error.Contains("blokada");
    }
}