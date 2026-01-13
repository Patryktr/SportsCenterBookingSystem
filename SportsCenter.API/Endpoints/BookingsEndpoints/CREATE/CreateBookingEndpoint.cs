using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
using SportsCenter.API.Extentions.Auth;
using SportsCenter.Application.Features.Bookings.CreateBooking;

namespace SportsCenter.API.Endpoints.BookingsEndpoints.CREATE;

public class CreateBookingEndpoint : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost($"{BookingsRoutes.Base}",
                async (CreateBookingRequest req, CreateBookingHandler h, CancellationToken ct) =>
                {
                    var result = await h.Handle(req, ct);
                    
                    if (result.IsSuccess)
                        return Results.Ok(result.Value);
                    
                    // Sprawdź czy błąd dotyczy konfliktu dostępności
                    var error = result.Error ?? "Nie można utworzyć rezerwacji";
                    if (IsAvailabilityConflict(error))
                        return Results.Conflict(new { error });
                    
                    return Results.BadRequest(new { error });
                })
            .RequireAuthorization(p => p.RequireRole(Roles.User, Roles.Admin))
            .WithName("CreateBooking")
            .WithTags("Bookings")
            .WithSummary("Tworzy nową rezerwację obiektu sportowego.")
            .WithDescription(
                "Tworzy nową rezerwację dla wskazanego obiektu sportowego w zadanym przedziale czasu. " +
                "Sprawdza poprawność dat, aktywność obiektu, limit graczy, godziny otwarcia, blokady terminów oraz konflikt terminów. " +
                "W przypadku konfliktu (obiekt już zarezerwowany, przerwa techniczna, poza godzinami otwarcia) zwraca status 409 Conflict.")
            .Produces<CreateBookingResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status409Conflict)
            .ProducesStandardErrors();
    }

    private static bool IsAvailabilityConflict(string error)
    {
        // Wykryj błędy związane z konfliktami dostępności
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