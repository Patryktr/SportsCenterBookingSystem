using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Xunit;

namespace SportsCenter.Tests.Performance;

// Definicja kolekcji dla testów wydajnościowych.
[CollectionDefinition("PerformanceTests", DisableParallelization = true)]
public class PerformanceTestsCollection : ICollectionFixture<PerformanceTestFixture>
{
}

// Fixture dla testów wydajnościowych - współdzielony kontekst.
public class PerformanceTestFixture : IDisposable
{
    public HttpClient AdminClient { get; }
    public HttpClient UserClient { get; }
    public string BaseUrl { get; }

    public PerformanceTestFixture()
    {
        BaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5001";
        AdminClient = CreateHttpClient("admin:123");
        UserClient = CreateHttpClient("user:123");
    }

    private HttpClient CreateHttpClient(string auth)
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

    public void Dispose()
    {
        AdminClient.Dispose();
        UserClient.Dispose();
    }
}

[Collection("PerformanceTests")]
public class ApiPerformanceTests : IClassFixture<PerformanceTestFixture>
{
    private readonly PerformanceTestFixture _fixture;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _userClient;

    public ApiPerformanceTests(PerformanceTestFixture fixture)
    {
        _fixture = fixture;
        _adminClient = fixture.AdminClient;
        _userClient = fixture.UserClient;
    }

    private static string UniqueCustomerId() => $"perf-test-{Guid.NewGuid()}";

    #region ==================== GET ENDPOINTS - LOAD TESTS ====================

    [Fact(Skip = "Requires running API server on localhost:5001")]
    [Trait("Category", "Performance")]
    public void GetFacilities_LoadTest()
    {
        var scenario = Scenario.Create("get_facilities", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("Accept", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/get-facilities")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 500, maxFailRate: 0.01);
    }

    [Fact(Skip = "Requires running API server on localhost:5001")]
    [Trait("Category", "Performance")]
    public void GetFacilityById_LoadTest()
    {
        var scenario = Scenario.Create("get_facility_by_id", async context =>
        {
            var facilityId = (context.InvocationNumber % 5) + 1;
            var request = Http.CreateRequest("GET", $"/api/facilities/{facilityId}")
                .WithHeader("Accept", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/get-facility-by-id")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 300, maxFailRate: 0.05);
    }

    [Fact(Skip = "Requires running API server on localhost:5001")]
    [Trait("Category", "Performance")]
    public void GetBookings_LoadTest()
    {
        var scenario = Scenario.Create("get_bookings", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings")
                .WithHeader("Accept", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/get-bookings")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 500, maxFailRate: 0.01);
    }

    [Fact(Skip = "Requires running API server on localhost:5001")]
    [Trait("Category", "Performance")]
    public void GetBookingById_LoadTest()
    {
        var scenario = Scenario.Create("get_booking_by_id", async context =>
        {
            var bookingId = (context.InvocationNumber % 10) + 1;
            var request = Http.CreateRequest("GET", $"/api/bookings/{bookingId}")
                .WithHeader("Accept", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/get-booking-by-id")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 300, maxFailRate: 0.10);
    }

    [Fact(Skip = "Requires running API server on localhost:5001")]
    [Trait("Category", "Performance")]
    public void GetCustomers_LoadTest()
    {
        var scenario = Scenario.Create("get_customers", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/customers")
                .WithHeader("Accept", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/get-customers")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 500, maxFailRate: 0.01);
    }

    #endregion

    #region ==================== AVAILABILITY ENDPOINTS - LOAD TESTS ====================

    [Fact(Skip = "Requires running API server on localhost:5001")]
    [Trait("Category", "Performance")]
    public void CheckAvailability_LoadTest()
    {
        var scenario = Scenario.Create("check_availability", async context =>
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var hour = 8 + (context.InvocationNumber % 12);
            var requestBody = new
            {
                facilityId = (context.InvocationNumber % 3) + 1,
                start = tomorrow.AddHours(hour).ToString("o"),
                end = tomorrow.AddHours(hour + 1).ToString("o")
            };

            var request = Http.CreateRequest("POST", "/api/availability/check")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/check-availability")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1000, maxFailRate: 0.05);
    }

    [Fact]
    public void SearchAvailability_LoadTest()
    {
        var scenario = Scenario.Create("search_availability", async context =>
        {
            var searchDate = DateTime.UtcNow.AddDays(1 + (context.InvocationNumber % 7)).Date;
            var requestBody = new
            {
                facilityId = (context.InvocationNumber % 3) + 1,
                date = searchDate.ToString("yyyy-MM-dd")
            };

            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/search-availability")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 2000, maxFailRate: 0.05);
    }

    #endregion

    #region ==================== CREATE ENDPOINTS - LOAD TESTS ====================

    [Fact]
    public void CreateCustomer_LoadTest()
    {
        var scenario = Scenario.Create("create_customer", async context =>
        {
            var requestBody = new
            {
                firstName = $"PerfTest{context.InvocationNumber}",
                lastName = "User",
                email = $"perf-{Guid.NewGuid()}@test.com",
                phone = "123456789"
            };

            var request = Http.CreateRequest("POST", "/api/customers")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/create-customer")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1000, maxFailRate: 0.01);
    }

    [Fact]
    public void CreateFacility_LoadTest()
    {
        var scenario = Scenario.Create("create_facility", async context =>
        {
            var requestBody = new
            {
                name = $"PerfTest Facility {Guid.NewGuid()}",
                sportType = (context.InvocationNumber % 5) + 1,
                maxPlayers = 4,
                pricePerHour = 100
            };

            var request = Http.CreateRequest("POST", "/api/facilities")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 3, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/create-facility")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1000, maxFailRate: 0.01);
    }

    [Fact]
    public void CreateBooking_LoadTest()
    {
        var scenario = Scenario.Create("create_booking", async context =>
        {
            var daysAhead = (context.InvocationNumber / 12) + 1;
            var hour = 8 + (context.InvocationNumber % 12);
            var bookingDate = DateTime.UtcNow.AddDays(daysAhead).Date;

            var requestBody = new
            {
                facilityId = 1,
                customerPublicId = Guid.NewGuid(),
                start = bookingDate.AddHours(hour).ToString("o"),
                end = bookingDate.AddHours(hour + 1).ToString("o"),
                playersCount = 2,
                type = 1
            };

            var request = Http.CreateRequest("POST", "/api/bookings")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/create-booking")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1500, maxFailRate: 0.50);
    }

    #endregion

    #region ==================== UPDATE ENDPOINTS - LOAD TESTS ====================

    [Fact]
    public void UpdateFacility_LoadTest()
    {
        var scenario = Scenario.Create("update_facility", async context =>
        {
            var facilityId = (context.InvocationNumber % 3) + 1;
            var requestBody = new
            {
                id = facilityId,
                name = $"Updated Facility {facilityId}",
                sportType = 1,
                maxPlayers = 4 + (context.InvocationNumber % 4),
                pricePerHour = 100 + (context.InvocationNumber % 50)
            };

            var request = Http.CreateRequest("PUT", $"/api/facilities/{facilityId}")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/update-facility")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1000, maxFailRate: 0.10);
    }

    [Fact]
    public void UpdateBooking_LoadTest()
    {
        var scenario = Scenario.Create("update_booking", async context =>
        {
            var bookingId = (context.InvocationNumber % 5) + 1;
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var hour = 10 + (context.InvocationNumber % 8);

            var requestBody = new
            {
                start = tomorrow.AddHours(hour).ToString("o"),
                end = tomorrow.AddHours(hour + 1).ToString("o"),
                playersCount = 2 + (context.InvocationNumber % 3)
            };

            var request = Http.CreateRequest("PUT", $"/api/bookings/{bookingId}")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/update-booking")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1500, maxFailRate: 0.50);
    }

    #endregion

    #region ==================== DELETE ENDPOINTS - LOAD TESTS ====================

    [Fact]
    public void DeleteBooking_LoadTest()
    {
        var scenario = Scenario.Create("delete_booking", async context =>
        {
            var bookingId = context.InvocationNumber + 1000;

            var request = Http.CreateRequest("DELETE", $"/api/bookings/{bookingId}")
                .WithHeader("X-Customer-Id", UniqueCustomerId());

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/delete-booking")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 500, maxFailRate: 0.95);
    }

    #endregion

    #region ==================== STRESS TESTS ====================

    [Fact]
    public void MixedReadOperations_StressTest()
    {
        var getFacilitiesScenario = Scenario.Create("stress_get_facilities", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var getBookingsScenario = Scenario.Create("stress_get_bookings", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var getCustomersScenario = Scenario.Create("stress_get_customers", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/customers")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var searchScenario = Scenario.Create("stress_search", async context =>
        {
            var searchDate = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new { facilityId = 1, date = searchDate.ToString("yyyy-MM-dd") };

            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 25, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(getFacilitiesScenario, getBookingsScenario, getCustomersScenario, searchScenario)
            .WithReportFolder("./reports/stress-mixed-read")
            .Run();

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            Assert.True(scenarioStats.Ok.Latency.Percent99 < 3000,
                $"Scenario '{scenarioStats.ScenarioName}' P99 latency ({scenarioStats.Ok.Latency.Percent99}ms) exceeds 3000ms");
        }
    }

    [Fact]
    public void MixedReadWriteOperations_StressTest()
    {
        var readScenario = Scenario.Create("stress_read", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var writeScenario = Scenario.Create("stress_write", async context =>
        {
            var requestBody = new
            {
                firstName = $"StressTest{context.InvocationNumber}",
                lastName = "User",
                email = $"stress-{Guid.NewGuid()}@test.com",
                phone = "123456789"
            };

            var request = Http.CreateRequest("POST", "/api/customers")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(readScenario, writeScenario)
            .WithReportFolder("./reports/stress-mixed-read-write")
            .Run();

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            Assert.True(scenarioStats.Ok.Latency.Percent99 < 5000,
                $"Scenario '{scenarioStats.ScenarioName}' P99 latency ({scenarioStats.Ok.Latency.Percent99}ms) exceeds 5000ms");
        }
    }

    [Fact]
    public void HighConcurrency_StressTest()
    {
        var scenario = Scenario.Create("high_concurrency", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/stress-high-concurrency")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 5000, maxFailRate: 0.10);
    }

    #endregion

    #region ==================== SPIKE TESTS ====================

    [Fact]
    public void PeakLoad_SpikeTest()
    {
        var scenario = Scenario.Create("spike_test", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/spike")
            .Run();

        Assert.True(stats.ScenarioStats[0].Ok.Latency.Percent99 < 5000,
            $"P99 latency ({stats.ScenarioStats[0].Ok.Latency.Percent99}ms) exceeds 5000ms during spike");
    }

    [Fact]
    public void DoubleSpike_StressTest()
    {
        var scenario = Scenario.Create("double_spike", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/bookings")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 40, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 60, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/double-spike")
            .Run();

        Assert.True(stats.ScenarioStats[0].Ok.Latency.Percent99 < 8000,
            $"P99 latency ({stats.ScenarioStats[0].Ok.Latency.Percent99}ms) exceeds 8000ms during double spike");
    }

    #endregion

    #region ==================== ENDURANCE TESTS ====================

    [Fact]
    public void Endurance_LongRunningTest()
    {
        var scenario = Scenario.Create("endurance_test", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/facilities")
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(5))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/endurance")
            .Run();

        var scenarioStats = stats.ScenarioStats[0];

        Assert.True(scenarioStats.Ok.Latency.Percent95 < 1000,
            $"P95 latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 1000ms");

        var totalRequests = scenarioStats.Ok.Request.Count + scenarioStats.Fail.Request.Count;
        var failRate = totalRequests > 0 ? (double)scenarioStats.Fail.Request.Count / totalRequests : 0;
        Assert.True(failRate < 0.01, $"Fail rate ({failRate:P}) exceeds 1%");
    }

    [Fact]
    public void Endurance_MixedOperations_LongRunningTest()
    {
        var readScenario = Scenario.Create("endurance_read", async context =>
        {
            var endpoint = (context.InvocationNumber % 3) switch
            {
                0 => "/api/facilities",
                1 => "/api/bookings",
                _ => "/api/customers"
            };
            var request = Http.CreateRequest("GET", endpoint)
                .WithHeader("X-Customer-Id", UniqueCustomerId());
            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
        );

        var searchScenario = Scenario.Create("endurance_search", async context =>
        {
            var searchDate = DateTime.UtcNow.AddDays(1 + (context.InvocationNumber % 30)).Date;
            var requestBody = new { facilityId = (context.InvocationNumber % 3) + 1, date = searchDate.ToString("yyyy-MM-dd") };

            var request = Http.CreateRequest("POST", "/api/availability/search")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
        );

        var stats = NBomberRunner
            .RegisterScenarios(readScenario, searchScenario)
            .WithReportFolder("./reports/endurance-mixed")
            .Run();

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            Assert.True(scenarioStats.Ok.Latency.Percent95 < 2000,
                $"Scenario '{scenarioStats.ScenarioName}' P95 latency ({scenarioStats.Ok.Latency.Percent95}ms) exceeds 2000ms");
        }
    }

    #endregion

    #region ==================== WORKFLOW TESTS ====================

    [Fact]
    public void RealisticUserWorkflow_Test()
    {
        var workflowScenario = Scenario.Create("user_workflow", async context =>
        {
            var step = context.InvocationNumber % 4;

            return step switch
            {
                0 => await Http.Send(_userClient, Http.CreateRequest("GET", "/api/facilities")
                    .WithHeader("X-Customer-Id", UniqueCustomerId())),

                1 => await Http.Send(_userClient, Http.CreateRequest("GET", $"/api/facilities/{(context.InvocationNumber % 3) + 1}")
                    .WithHeader("X-Customer-Id", UniqueCustomerId())),

                2 => await Http.Send(_userClient, Http.CreateRequest("POST", "/api/availability/search")
                    .WithHeader("Content-Type", "application/json")
                    .WithHeader("X-Customer-Id", UniqueCustomerId())
                    .WithBody(new StringContent(
                        JsonSerializer.Serialize(new
                        {
                            facilityId = 1,
                            date = DateTime.UtcNow.AddDays(1).Date.ToString("yyyy-MM-dd")
                        }),
                        Encoding.UTF8, "application/json"))),

                _ => await Http.Send(_userClient, Http.CreateRequest("POST", "/api/availability/check")
                    .WithHeader("Content-Type", "application/json")
                    .WithHeader("X-Customer-Id", UniqueCustomerId())
                    .WithBody(new StringContent(
                        JsonSerializer.Serialize(new
                        {
                            facilityId = 1,
                            start = DateTime.UtcNow.AddDays(1).Date.AddHours(10).ToString("o"),
                            end = DateTime.UtcNow.AddDays(1).Date.AddHours(11).ToString("o")
                        }),
                        Encoding.UTF8, "application/json")))
            };
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(workflowScenario)
            .WithReportFolder("./reports/user-workflow")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 2000, maxFailRate: 0.05);
    }

    [Fact]
    public void AdminWorkflow_Test()
    {
        var workflowScenario = Scenario.Create("admin_workflow", async context =>
        {
            var step = context.InvocationNumber % 3;

            return step switch
            {
                0 => await Http.Send(_adminClient, Http.CreateRequest("GET", "/api/customers")
                    .WithHeader("X-Customer-Id", UniqueCustomerId())),

                1 => await Http.Send(_adminClient, Http.CreateRequest("GET", "/api/bookings")
                    .WithHeader("X-Customer-Id", UniqueCustomerId())),

                _ => await Http.Send(_adminClient, Http.CreateRequest("GET", "/api/facilities")
                    .WithHeader("X-Customer-Id", UniqueCustomerId()))
            };
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(workflowScenario)
            .WithReportFolder("./reports/admin-workflow")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 1500, maxFailRate: 0.02);
    }

    #endregion

    #region ==================== CONCURRENT WRITE TESTS ====================

    [Fact]
    public void ConcurrentBookingCreation_ConflictTest()
    {
        var scenario = Scenario.Create("concurrent_booking_conflict", async context =>
        {
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var requestBody = new
            {
                facilityId = 1,
                customerPublicId = Guid.NewGuid(),
                start = tomorrow.AddHours(10).ToString("o"),
                end = tomorrow.AddHours(11).ToString("o"),
                playersCount = 2,
                type = 1
            };

            var request = Http.CreateRequest("POST", "/api/bookings")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/concurrent-booking-conflict")
            .Run();

        Assert.True(stats.ScenarioStats[0].Ok.Latency.Percent99 < 5000,
            "System should handle concurrent conflicts without timing out");
    }

    [Fact]
    public void ConcurrentCustomerCreation_Test()
    {
        var scenario = Scenario.Create("concurrent_customer_creation", async context =>
        {
            var requestBody = new
            {
                firstName = $"Concurrent{context.InvocationNumber}",
                lastName = "Test",
                email = $"concurrent-{Guid.NewGuid()}@test.com",
                phone = "123456789"
            };

            var request = Http.CreateRequest("POST", "/api/customers")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-Customer-Id", UniqueCustomerId())
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_adminClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/concurrent-customer-creation")
            .Run();

        AssertPerformance(stats.ScenarioStats[0], maxP95Latency: 2000, maxFailRate: 0.05);
    }

    #endregion

    #region ==================== HELPER METHODS ====================

    private static void AssertPerformance(ScenarioStats stats, double maxP95Latency, double maxFailRate)
    {
        Assert.True(stats.Ok.Latency.Percent95 < maxP95Latency,
            $"P95 latency ({stats.Ok.Latency.Percent95}ms) exceeds {maxP95Latency}ms");

        var totalRequests = stats.Ok.Request.Count + stats.Fail.Request.Count;
        var failRate = totalRequests > 0 ? (double)stats.Fail.Request.Count / totalRequests : 0;
        Assert.True(failRate < maxFailRate,
            $"Fail rate ({failRate:P}) exceeds {maxFailRate:P}");
    }

    #endregion
}