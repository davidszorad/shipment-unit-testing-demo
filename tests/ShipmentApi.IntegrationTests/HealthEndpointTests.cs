using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ShipmentApi.IntegrationTests;

[TestClass]
public sealed class HealthEndpointTests
{
    [TestMethod]
    public async Task GetHealth_Always_ReturnsOk()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
