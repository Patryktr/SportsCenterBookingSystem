using SportsCenter.Application.Features.Facilities.CreateFacility;
using SportsCenter.Application.Features.Facilities.GetFacilities;
using SportsCenter.Application.Features.Facilities.GetFacilityById;
using SportsCenter.Application.Features.Facilities.UpdateFacility;
using SportsCenter.Application.Features.Facilities.DeleteFacility;

public static class FacilitiesEndpoints
{
    public static void MapFacilitiesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/facilities");

        group.MapPost("/", async (CreateFacilityRequest req, CreateFacilityHandler h, CancellationToken ct)
            => Results.Ok(await h.Handle(req, ct)));

        group.MapGet("/", async (GetFacilitiesHandler h, CancellationToken ct)
            => Results.Ok(await h.Handle(ct)));

        group.MapGet("/{id:int}", async (int id, GetFacilityByIdHandler h, CancellationToken ct)
            => (await h.Handle(id, ct)) is { } f ? Results.Ok(f) : Results.NotFound());

        group.MapPut("/{id:int}", async (int id, UpdateFacilityRequest req, UpdateFacilityHandler h, CancellationToken ct)
            => id != req.Id ? Results.BadRequest() :
            await h.Handle(req, ct) ? Results.Ok() : Results.NotFound());

        group.MapDelete("/{id:int}", async (int id, DeleteFacilityHandler h, CancellationToken ct)
            => await h.Handle(id, ct) ? Results.Ok() : Results.NotFound());
    }
}