using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace SportsCenter.Tests.Performance;

/// <summary>
/// Testy wydajnościowe API przy użyciu NBomber.
/// 
/// PRZED URUCHOMIENIEM:
/// 1. Upewnij się że API działa: docker-compose up -d
/// 2. Sprawdź czy API odpowiada: curl http://localhost:5001/api/facilities
/// 
/// URUCHOMIENIE WSZYSTKICH TESTÓW WYDAJNOŚCIOWYCH:
///   dotnet test --filter "FullyQualifiedName~ApiPerformanceTests" --no-build
/// </summary>
[Collection("PerformanceTests")]
public class ApiPerformanceTests : IDisposable
{
    private const string BaseUrl = "http://localhost:5001";
    private const string AdminAuth = "admin:123";
    private readonly HttpClient _httpClient;

    public ApiPerformanceTests()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        var authBytes = Encoding.UTF8.GetBytes(AdminAuth);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    [Fact]
    public void GetFacilities_LoadTest()
    {
        var scenario = Scenario.Create("get_facilities", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("Accept", "application/json");
            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/facilities")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 500, maxFailRate: 0.01);
    }

    [Fact]
    public void GetBookings_LoadTest()
    {
        var scenario = Scenario.Create("get_bookings", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings")
                .WithHeader("Accept", "application/json");
            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/bookings")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 500, maxFailRate: 0.01);
    }

    [Fact]
    public void CheckAvailability_LoadTest()
    {
        var scenario = Scenario.Create("check_availability", async context =>
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new
            {
                facilityId = 1,
                start = tomorrow.AddHours(10).ToString("o"),
                end = tomorrow.AddHours(12).ToString("o")
            };

            var request = Http.CreateRequest("POST", "/api/availability/check")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    Encoding.UTF8, 
                    "application/json"));

            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/availability-check")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1000, maxFailRate: 0.01);
    }

    [Fact]
    public void SearchAvailability_LoadTest()
    {
        var scenario = Scenario.Create("search_availability", async context =>
        {
            var searchDate = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new
            {
                facilityId = 1,
                date = searchDate.ToString("yyyy-MM-dd")
            };

            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    Encoding.UTF8, 
                    "application/json"));

            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/availability-search")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 2000, maxFailRate: 0.01);
    }

    [Fact]
    public void MixedOperations_StressTest()
    {
        var getFacilitiesScenario = Scenario.Create("mixed_get_facilities", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities");
            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var getBookingsScenario = Scenario.Create("mixed_get_bookings", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings");
            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var searchScenario = Scenario.Create("mixed_search", async context =>
        {
            var searchDate = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new { facilityId = 1, date = searchDate.ToString("yyyy-MM-dd") };
            
            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            
            return await Http.Send(_httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(getFacilitiesScenario, getBookingsScenario, searchScenario)
            .WithReportFolder("./reports/mixed-stress")
            .Run();

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            Assert.True(scenarioStats.Ok.Latency.Percent99 < 3000, 
                $"Scenario '{scenarioStats.ScenarioName}' P99 latency ({scenarioStats.Ok.Latency.Percent99}ms) exceeds 3000ms");
        }
    }

    private static void AssertPerformance(ScenarioStats stats, double maxP95Latency, double maxFailRate)
    {
        Assert.True(stats.Ok.Latency.Percent95 < maxP95Latency, 
            $"P95 latency ({stats.Ok.Latency.Percent95}ms) exceeds {maxP95Latency}ms");
        
        var totalRequests = stats.Ok.Request.Count + stats.Fail.Request.Count;
        var failRate = totalRequests > 0 ? (double)stats.Fail.Request.Count / totalRequests : 0;
        Assert.True(failRate < maxFailRate, 
            $"Fail rate ({failRate:P}) exceeds {maxFailRate:P}");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}