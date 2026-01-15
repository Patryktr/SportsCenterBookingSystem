using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace SportsCenter.Tests.Performance;

public class ApiPerformanceTests
{
    private const string BaseUrl = "http://localhost:5001";
    private const string AdminAuth = "admin:123";
    private const string UserAuth = "user:123";
    
    private static HttpClient CreateHttpClient(string auth = AdminAuth)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        var authBytes = Encoding.UTF8.GetBytes(auth);
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        
        return client;
    }
    
    // Test wydajności endpointa GET /api/facilities
    // Symuluje 10 użytkowników przez 30 sekund
    [Fact]
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
        var totalRequests = scenarioStats.Ok.Request.Count + scenarioStats.Fail.Request.Count;
        var failRate = totalRequests > 0 ? (double)scenarioStats.Fail.Request.Count / totalRequests : 0;
        Assert.True(failRate < 0.01, $"Fail rate ({failRate:P}) exceeds 1%");
    }
    
    // Test wydajności endpointa POST /api/availability/check
    // Symuluje sprawdzanie dostępności przez wielu użytkowników
    [Fact]
    public void CheckAvailability_LoadTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("check_availability", async context =>
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new
            {
                facilityId = 1,
                start = tomorrow.AddHours(10).ToString("o"), // 10:00 - pełna godzina
                end = tomorrow.AddHours(12).ToString("o")    // 12:00 - pełna godzina
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
            .WithReportFolder("./reports/availability-check")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 1000, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 1000ms");
    }
    
    // Test wydajności endpointa POST /api/availability/search
    // Wyszukiwanie wolnych slotów godzinowych dla konkretnego obiektu
    [Fact]
    public void SearchAvailability_LoadTest()
    {
        using var httpClient = CreateHttpClient();

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

            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/availability-search")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 2000, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 2000ms");
    }
    
    // Test wydajności GET /api/bookings
    [Fact]
    public void GetBookings_LoadTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("get_bookings", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings")
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
            .WithReportFolder("./reports/bookings")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 500, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 500ms");
    }
    
    // Test wydajności GET /api/customers (tylko admin)
    [Fact]
    public void GetCustomers_LoadTest()
    {
        using var httpClient = CreateHttpClient(AdminAuth);

        var scenario = Scenario.Create("get_customers", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/customers")
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
            .WithReportFolder("./reports/customers")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 500, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 500ms");
    }
    
    // Test obciążeniowy - mieszane operacje
    // Symuluje realistyczny ruch użytkowników przez 60 sekund
    [Fact]
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

        var searchAvailabilityScenario = Scenario.Create("search_availability", async context =>
        {
            var searchDate = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new { facilityId = 1, date = searchDate.ToString("yyyy-MM-dd") };
            
            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(getFacilitiesScenario, getBookingsScenario, searchAvailabilityScenario)
            .WithReportFolder("./reports/mixed-stress")
            .Run();

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            Assert.True(scenarioStats.Ok.Latency.Percent99 < 3000, 
                $"Scenario '{scenarioStats.ScenarioName}' 99th percentile latency ({scenarioStats.Ok.Latency.Percent99}ms) exceeds 3000ms");
        }
    }
    
    // Test szczytowego obciążenia - symuluje nagły wzrost ruchu
    [Fact]
    public void PeakLoad_SpikeTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("spike_test", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            // Normalne obciążenie przez 10 sekund
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            // Skok do 50 req/s przez 10 sekund
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            // Powrót do normalnego obciążenia
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/spike")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        // Podczas spike'a akceptujemy wyższe latency, ale nadal < 5s
        Assert.True(scenarioStats.Ok.Latency.Percent99 < 5000, 
            $"99th percentile latency ({scenarioStats.Ok.Latency.Percent99}ms) exceeds 5000ms during spike");
    }
    
    // Test długotrwałego obciążenia - stabilność systemu
    [Fact]
    public void Endurance_LongRunningTest()
    {
        using var httpClient = CreateHttpClient();

        var scenario = Scenario.Create("endurance_test", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            // Stałe obciążenie przez 5 minut
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(5))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/endurance")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];
        
        // Sprawdź stabilność - latency nie powinno rosnąć
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 1000, 
            $"95th percentile latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 1000ms - possible memory leak or degradation");
        
        // Brak błędów
        var totalRequests = scenarioStats.Ok.Request.Count + scenarioStats.Fail.Request.Count;
        var failRate = totalRequests > 0 ? (double)scenarioStats.Fail.Request.Count / totalRequests : 0;
        Assert.True(failRate < 0.001, $"Fail rate ({failRate:P}) exceeds 0.1% during endurance test");
    }
}