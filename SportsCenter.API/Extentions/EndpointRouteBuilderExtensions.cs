using System.Reflection;

namespace SportsCenter.API.Extentions;
public static class EndpointRouteBuilderExtensions
{
    public static void MapDiscoveredEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointDefinitionType = typeof(IEndpointDefinition);

       
        var definitions = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                endpointDefinitionType.IsAssignableFrom(t))
            .Select(Activator.CreateInstance)
            .Cast<IEndpointDefinition>()
            .ToList();

        foreach (var def in definitions)
        {
            def.RegisterEndpoints(app);
        }
    }
}
