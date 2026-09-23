using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration;

public class ProblemDetailsTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    #region Shape

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
        document.Should().Contain("/api/orders");
        scalar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Codes

    [Fact]
    public async Task A_rejected_field_carries_our_code_beside_its_message()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock) with { CustomerName = "", PhoneNumber = "12" };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await ReadAsync(response);
        problem.GetProperty("code").GetString().Should().Be("InvalidRequest");
        var codes = problem.GetProperty("errorCodes");
        codes.GetProperty("customerName")[0].GetString().Should().Be("Required");
        codes.GetProperty("phoneNumber")[0].GetString().Should().Be("InvalidPhoneNumber");
        problem.GetProperty("errors").GetProperty("phoneNumber")[0].GetString().Should().Contain("digits");
    }

    [Fact]
    public async Task A_broken_domain_rule_carries_its_code()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/due-date",
            new RescheduleOrderRequest(Clock.Today.AddDays(-1), order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        (await ReadAsync(response)).GetProperty("code").GetString().Should().Be("DueDateInPast");
    }

    [Fact]
    public async Task A_stale_edit_carries_its_code()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/due-date",
            new RescheduleOrderRequest(Clock.Today.AddDays(5), order.Version + 1),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await ReadAsync(response)).GetProperty("code").GetString().Should().Be("OrderChanged");
    }

    #endregion

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }
}
