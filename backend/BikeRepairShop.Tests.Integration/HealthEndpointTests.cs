using System.Net;
using AwesomeAssertions;
using BikeRepairShop.Tests.Integration.Fixtures;

namespace BikeRepairShop.Tests.Integration;

public class HealthEndpointTests(DatabaseFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Be("Healthy");
    }
}
