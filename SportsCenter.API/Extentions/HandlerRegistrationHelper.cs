using SportsCenter.Application.Abstractions;

namespace SportsCenter.API.Extentions;

    public static  class HandlerRegistrationHelper
    {
        public static void RegisterDiscoveredHandlers(this IServiceCollection services)
        {
            var defType = typeof(IHandlerDefinition);

            var definitions = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    defType.IsAssignableFrom(t));

            foreach (var def in definitions)
            {
                services.AddScoped(def);
            }
        }
    }

