using Microsoft.EntityFrameworkCore;
using SportsCenter.API.Extensions.Auth;
using SportsCenter.API.Extensions.RateLimiterConfig;
using SportsCenter.API.Extentions;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // DbContext + SQL Server
        builder.Services.AddDbContext<SportsCenterDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SportsCenterDb"));
        });
        
        // Rate Limiter
        builder.Services.AddCustomRateLimiter();

        // Rejestracja handlerów
        builder.Services.RegisterDiscoveredHandlers();

        // Swagger / OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Auth
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddApiAuth(builder.Configuration);

        var app = builder.Build();

        // Automatyczne tworzenie / aktualizacja bazy danych przy starcie
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SportsCenterDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRateLimiter();

        app.UseHttpsRedirection();

        app.UseApiAuth();

        app.MapDiscoveredEndpoints();

        app.Run();
    }
}