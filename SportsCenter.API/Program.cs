using Microsoft.EntityFrameworkCore;
using SportsCenter.API.Extentions;
using SportsCenter.Infrastructure.Persistence;
using SportsCenter.Application.Features.Customers.CreateCustomer;

namespace SportsCenter.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // DbContext + SQLite
        builder.Services.AddDbContext<SportsCenterDbContext>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("SportsCenterDb"));
        });
        builder.Services.AddScoped<CreateCustomerHandler>();
        builder.Services.AddScoped<CreateFacilityHandler>();
        builder.Services.AddScoped<GetFacilitiesHandler>();
        builder.Services.AddScoped<GetFacilityByIdHandler>();
        builder.Services.AddScoped<UpdateFacilityHandler>();
        builder.Services.AddScoped<DeleteFacilityHandler>();


        builder.Services.RegisterDiscoveredHandlers();

        // Swagger / OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers(); // na p�niej, gdy dodamy kontrolery

        app.MapDiscoveredEndpoints();

        // Na razie prosty endpoint testowy
        app.MapGet("/ping", () => "pong");

        app.Run();
    }
}