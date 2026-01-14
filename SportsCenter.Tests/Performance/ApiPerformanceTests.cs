using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace SportsCenter.Tests.Performance;

/// <summary>
/// Testy wydajnościowe API przy użyciu NBomber.
/// 
/// UWAGA: Te testy wymagają uruchomionej instancji API.
/// Przed uruchomieniem upewnij się, że:
/// 1. Aplikacja działa pod adresem http://localhost:5001
/// 2. W bazie istnieją dane testowe (Facility, Customer)
/// 
/// Uruchomienie: dotnet test --filter "FullyQualifiedName~Performance"
/// </summary>
public class ApiPerformanceTests
{
    private const string BaseUrl = "http://localhost:5001";
    private const string AdminAuth = "admin:123";
    
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
        
        var authBytes = Encoding.UTF8.GetBytes(AdminAuth);
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        
        return client;
    }

    /// <summary>
    /// Test wydajności endpointa GET /api/facilities
    /// Symuluje 10 użytkowników przez 30 sekund
    /// </summary>
    [Fact(Skip = "Wymaga uruchomionej instancji API")]
    public void GetFacilities_LoadTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("get_facilities", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("Accept", "application/json");

            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/facilities")
            .Run();

        // Assertions
        var scenarioStats = stats.ScenarioStats[0];
        
        // 95% percentyl powinien być poniżej 500ms
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 500, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 500ms");
        
        // Mniej niż 1% błędów
        var failRate = (double)scenarioStats.Fail.Request.Count / scenarioStats.Ok.Request.Count;
        Assert.True(failRate < 0.01, $"Fail rate ({failRate:P}) exceeds 1%");
    }

    /// <summary>
    /// Test wydajności endpointa POST /api/availability/check
    /// Symuluje sprawdzanie dostępności przez wielu użytkowników
    /// </summary>
    [Fact(Skip = "Wymaga uruchomionej instancji API")]
    public void CheckAvailability_LoadTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("check_availability", async context =>
        {
            var requestBody = new
            {
                facilityId = 1,
                start = DateTime.UtcNow.AddDays(1).ToString("o"),
                end = DateTime.UtcNow.AddDays(1).AddHours(2).ToString("o")
            };

            var request = Http.CreateRequest("POST", "/api/availability/check")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    Encoding.UTF8, 
                    "application/json"));

            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/availability")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 1000, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 1000ms");
    }

    /// <summary>
    /// Test wydajności endpointa POST /api/availability/search
    /// Symuluje wyszukiwanie wolnych terminów
    /// </summary>
    [Fact(Skip = "Wymaga uruchomionej instancji API")]
    public void SearchAvailability_LoadTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("search_availability", async context =>
        {
            var requestBody = new
            {
                start = DateTime.UtcNow.AddDays(1).ToString("o"),
                end = DateTime.UtcNow.AddDays(1).AddHours(2).ToString("o"),
                sportType = (int?)null,
                minPlayers = (int?)null
            };

            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    Encoding.UTF8, 
                    "application/json"));

            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/search")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 2000, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 2000ms");
    }

    /// <summary>
    /// Test obciążeniowy - mieszane operacje
    /// Symuluje realistyczny ruch użytkowników
    /// </summary>
    [Fact(Skip = "Wymaga uruchomionej instancji API")]
    public void MixedOperations_StressTest()
    {
        using var httpClient = CreateHttpClient();

        var getFacilitiesScenario = Scenario.Create("get_facilities", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var getBookingsScenario = Scenario.Create("get_bookings", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(getFacilitiesScenario, getBookingsScenario)
            .WithReportFolder("./reports/mixed")
            .Run();

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            Assert.True(scenarioStats.Ok.Latency.Percent99 < 3000, 
                $"Scenario '{scenarioStats.ScenarioName}' 99th percentile latency exceeds 3000ms");
        }
    }
}