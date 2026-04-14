using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace QuantityMeasurementApi.Tests;

[TestClass]
public sealed class QuantityMeasurementControllerTests
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static void Setup(TestContext _)
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task Compare_Returns200AndResult()
    {
        var request = new
        {
            thisQuantityDTO = new { value = 1.0, unit = "FEET", measurementType = "LengthUnit" },
            thatQuantityDTO = new { value = 12.0, unit = "INCHES", measurementType = "LengthUnit" }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/quantities/compare", request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "\"operation\":\"compare\"");
        StringAssert.Contains(body, "\"resultString\":\"true\"");
    }

    [TestMethod]
    public async Task Compare_InvalidInput_Returns400()
    {
        var request = new
        {
            thisQuantityDTO = new { value = 1.0, unit = "FEET", measurementType = "LengthUnit" },
            thatQuantityDTO = new { value = 12.0, unit = "INCHE", measurementType = "LengthUnit" }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/quantities/compare", request);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(body, "\"error\":\"Quantity Measurement Error\"");
    }

    [TestMethod]
    public async Task CountEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/quantities/count/COMPARE");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
