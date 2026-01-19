using Microsoft.EntityFrameworkCore;
using SportsCenter.API.Extensions.Auth;
using SportsCenter.API.Extensions.RateLimiterConfig;
using SportsCenter.API.Extentions;
using SportsCenter.Application.Cache;
using SportsCenter.Application.Services;
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
        
        // Rejestracja serwisów
        builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

        // Swagger / OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Auth
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddApiAuth(builder.Configuration);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
        });

        builder.Services.AddSingleton<ICacheService, DistributedCacheService>();

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