using Microsoft.EntityFrameworkCore;
using SportsCenter.Infrastructure.Persistence;

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

     
        builder.Services.AddControllers();

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

        app.MapControllers(); // na póŸniej, gdy dodamy kontrolery

        // Na razie prosty endpoint testowy
        app.MapGet("/ping", () => "pong");

        app.Run();
    }
}