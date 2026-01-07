using SportsCenter.API.Endpoints.Common;
using SportsCenter.API.Extentions;
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
            .WithTags("Bookings")
            .WithSummary("Tworzy nową rezerwację obiektu sportowego.")
            .WithDescription(
                "Tworzy nową rezerwację dla wskazanego obiektu sportowego w zadanym przedziale czasu. " +
                "Sprawdza poprawność dat, aktywność obiektu, limit graczy oraz konflikt terminów. " +
                "W przypadku powodzenia zwraca szczegóły utworzonej rezerwacji.")
            .Produces<CreateBookingResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesStandardErrors();
    }
}