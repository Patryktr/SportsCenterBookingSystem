using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;


namespace SportsCenter.API.Extensions.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddApiAuth(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddAuthentication("Basic")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", _ => { });

        services.AddAuthorization();

        // Swagger: Basic Auth “kłódka”
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Description = "Basic Auth: user/pass"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "basic"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseApiAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
