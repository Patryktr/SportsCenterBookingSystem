using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SportsCenter.API.Endpoints.Common;

public static class EndpointConventions
{
    public static RouteHandlerBuilder WithStandardSwagger(
        this RouteHandlerBuilder builder,
        string name,
        string tag,
        string summary,
        string description)
        => builder
            .WithName(name)
            .WithTags(tag)
            .WithSummary(summary)
            .WithDescription(description);

    public static RouteHandlerBuilder ProducesStandardErrors(
        this RouteHandlerBuilder builder)
        => builder
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError);
}
