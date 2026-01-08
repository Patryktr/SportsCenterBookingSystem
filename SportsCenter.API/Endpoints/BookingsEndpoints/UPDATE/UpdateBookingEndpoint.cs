using SportsCenter.API.Endpoints.Common;
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
            .WithTags("Bookings")
            .WithSummary("Aktualizuje rezerwacjê.")
            .WithDescription(
                "Aktualizuje dane istniej¹cej rezerwacji na podstawie identyfikatora w URL. " +
                "Weryfikuje poprawnoœæ danych wejœciowych oraz regu³y biznesowe (np. terminy i dostêpnoœæ). " +
                "Je¿eli aktualizacja siê powiedzie, zwracany jest status 200.")
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardErrors();
    }
}