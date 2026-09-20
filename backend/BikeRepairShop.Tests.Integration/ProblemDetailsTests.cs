using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Tests.Integration;

public class ProblemDetailsTests(DatabaseFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task An_unknown_route_returns_problem_details()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/nothing-here", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Status.Should().Be(404);
    }

    [Fact]
    public async Task The_openapi_document_and_scalar_are_served()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var document = await client.GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        var scalar = await client.GetAsync("/scalar", TestContext.Current.CancellationToken);

        // Assert
        document.Should().Contain("\"openapi\"");
        scalar.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
