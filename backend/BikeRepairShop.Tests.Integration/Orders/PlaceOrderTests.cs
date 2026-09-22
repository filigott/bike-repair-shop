using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Database.Configurations;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.Orders;

public class PlaceOrderTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    [Fact]
    public async Task Placing_an_order_returns_it_as_received_and_persists_it()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock);

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken);
        response.Headers.Location!.AbsolutePath.Should().Be($"/api/orders/{body!.Id}");
        body.Status.Should().Be(OrderStatus.Received);
        body.Email.Should().Be("kari@example.com");
        body.PhoneNumber.Should().Be("+4791234567");
        body.AllowedTransitions.Should().BeEquivalentTo([OrderStatus.InProgress, OrderStatus.Completed, OrderStatus.Cancelled]);

        await using var scope = Database.CreateScope();
        var saved = await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.SingleAsync(o => o.Id == body.Id, TestContext.Current.CancellationToken);
        saved.CustomerName.Should().Be(request.CustomerName);
        saved.ServiceType.Should().Be(request.ServiceType);
        saved.DueDate.Should().Be(request.DueDate!.Value);
        saved.Notes.Should().Be(request.Notes);
    }

    [Fact]
    public async Task Placing_an_order_with_malformed_fields_returns_one_error_per_field()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock) with { CustomerName = "", PhoneNumber = "12", Email = "kari", BikeBrand = null! };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("customerName", "phoneNumber", "email", "bikeBrand");
        problem.Errors.Values.Should().AllSatisfy(messages => messages.Should().ContainSingle());
    }

    [Fact]
    public async Task Placing_an_order_assigns_an_order_number_that_the_response_carries()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock);

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken);
        body!.OrderNumber.Should().BeGreaterThanOrEqualTo(MaintenanceOrderConfiguration.OrderNumberStartValue);

        await using var scope = Database.CreateScope();
        var saved = await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.SingleAsync(o => o.Id == body.Id, TestContext.Current.CancellationToken);
        saved.OrderNumber.Should().Be(body.OrderNumber);
    }

    [Fact]
    public async Task Placing_a_second_order_assigns_it_a_higher_order_number()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock);

        // Act
        var first = await PlaceAsync(client, request);
        var second = await PlaceAsync(client, request);

        // Assert
        second.OrderNumber.Should().BeGreaterThan(first.OrderNumber);
    }

    [Fact]
    public async Task Placing_an_order_without_a_due_date_reports_the_field_rather_than_the_binder()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock) with { DueDate = null };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("dueDate");
    }

    [Fact]
    public async Task Placing_an_order_due_in_the_past_is_rejected_by_the_domain()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var request = PlaceOrderRequests.Valid(Clock, dueInDays: -1);

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Detail.Should().Be("The due date cannot be before today.");
    }

    private static async Task<MaintenanceOrderResponse> PlaceAsync(HttpClient client, PlaceOrderRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/orders", request, Json.Options, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken))!;
    }
}
