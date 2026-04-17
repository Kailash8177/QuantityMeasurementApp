using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace QuantityMeasurementApi.Tests;

[TestClass]
public sealed class QuantityMeasurementControllerTests
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static HttpClient _client     = null!;
    private static HttpClient _anonClient = null!;
    private static string _adminToken     = string.Empty;

    [ClassInitialize]
    public static async Task Setup(TestContext _)
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:Key"]                    = "ThisIsASecretKeyForQuantityMeasurementJwtAuth2024!AtLeast32Chars",
                        ["Jwt:Issuer"]                 = "QuantityMeasurementApi",
                        ["Jwt:Audience"]               = "QuantityMeasurementClient",
                        ["Jwt:TokenExpiryInMinutes"]   = "60",
                        ["Jwt:RefreshTokenExpiryInDays"] = "7"
                    });
                });
            });

        _anonClient = _factory.CreateClient();
        _client     = _factory.CreateClient();

        // Register a test user
        await _anonClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username  = "testuser",
            email     = "testuser@test.com",
            password  = "Test@1234",
            firstName = "Test",
            lastName  = "User"
        });

        // Login as seeded admin
        var loginResp = await _anonClient.PostAsJsonAsync("/api/v1/auth/login", new
        {
            username = "admin",
            password = "admin123"
        });

        if (loginResp.StatusCode == HttpStatusCode.OK)
        {
            var body = await loginResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            _adminToken = body!["accessToken"]?.ToString() ?? string.Empty;
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _adminToken);
        }
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        _client.Dispose();
        _anonClient.Dispose();
        _factory.Dispose();
    }

    // ─── Auth Tests ─────────────────────────────────────────────

    [TestMethod]
    public async Task Register_NewUser_Returns200()
    {
        var resp = await _anonClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username  = "newuser2",
            email     = "newuser2@test.com",
            password  = "Pass@1234",
            firstName = "New",
            lastName  = "User"
        });
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "accessToken");
    }

    [TestMethod]
    public async Task Register_DuplicateUsername_Returns400()
    {
        // Register once
        await _anonClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "dupuser", email = "dup1@test.com", password = "Pass@1234"
        });
        // Register again with same username
        var resp = await _anonClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "dupuser", email = "dup2@test.com", password = "Pass@1234"
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [TestMethod]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var resp = await _anonClient.PostAsJsonAsync("/api/v1/auth/login", new
        {
            username = "testuser",
            password = "Test@1234"
        });
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "accessToken");
    }

    [TestMethod]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var resp = await _anonClient.PostAsJsonAsync("/api/v1/auth/login", new
        {
            username = "admin",
            password = "wrongpassword"
        });
        Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [TestMethod]
    public async Task AuthStatus_Anonymous_ReturnsNotAuthenticated()
    {
        var resp = await _anonClient.GetAsync("/api/v1/auth/status");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "false");
    }

    [TestMethod]
    public async Task GetProfile_WithToken_Returns200()
    {
        var resp = await _client.GetAsync("/api/v1/auth/profile");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "admin");
    }

    // ─── Admin Tests ─────────────────────────────────────────────

    [TestMethod]
    public async Task Admin_GetUsers_AsAdmin_Returns200()
    {
        var resp = await _client.GetAsync("/api/v1/admin/users");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    public async Task Admin_GetUsers_WithoutToken_Returns401()
    {
        var resp = await _anonClient.GetAsync("/api/v1/admin/users");
        Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [TestMethod]
    public async Task Admin_Statistics_AsAdmin_Returns200()
    {
        var resp = await _client.GetAsync("/api/v1/admin/statistics");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "TotalUsers");
    }

    // ─── Quantity Tests ──────────────────────────────────────────

    [TestMethod]
    public async Task Quantity_WithoutToken_Returns401()
    {
        var resp = await _anonClient.PostAsJsonAsync("/api/v1/quantities/compare", new
        {
            thisQuantityDTO = new { value = 1.0, unit = "FEET", measurementType = "LengthUnit" },
            thatQuantityDTO = new { value = 12.0, unit = "INCHES", measurementType = "LengthUnit" }
        });
        Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [TestMethod]
    public async Task Compare_Returns200AndResult()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/quantities/compare", new
        {
            thisQuantityDTO = new { value = 1.0, unit = "FEET", measurementType = "LengthUnit" },
            thatQuantityDTO = new { value = 12.0, unit = "INCHES", measurementType = "LengthUnit" }
        });
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "\"operation\":\"compare\"");
        StringAssert.Contains(body, "\"resultString\":\"true\"");
    }

    [TestMethod]
    public async Task Compare_InvalidUnit_Returns400()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/quantities/compare", new
        {
            thisQuantityDTO = new { value = 1.0, unit = "FEET", measurementType = "LengthUnit" },
            thatQuantityDTO = new { value = 12.0, unit = "INVALID_UNIT", measurementType = "LengthUnit" }
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [TestMethod]
    public async Task Count_Returns200()
    {
        var resp = await _client.GetAsync("/api/v1/quantities/count/COMPARE");
        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }
}
