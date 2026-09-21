using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.Orders;

public class GetOrderTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    [Fact]
    public async Task Getting_an_order_returns_it_with_the_flags_the_client_must_not_recompute()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, customerName: "Kari Nordmann", dueInDays: 3);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var body = await client.GetFromJsonAsync<MaintenanceOrderResponse>(
            $"/api/orders/{order.Id}", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        body!.CustomerName.Should().Be("Kari Nordmann");
        body.Status.Should().Be(OrderStatus.Received);
        body.IsOverdue.Should().BeFalse();
        body.IsEditable.Should().BeTrue();
        body.AllowedTransitions.Should().BeEquivalentTo([OrderStatus.InProgress, OrderStatus.Completed, OrderStatus.Cancelled]);
    }

    [Fact]
    public async Task A_completed_order_is_reported_as_no_longer_editable()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Complete();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var body = await client.GetFromJsonAsync<MaintenanceOrderResponse>(
            $"/api/orders/{order.Id}", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        body!.IsEditable.Should().BeFalse();
        body.AllowedTransitions.Should().BeEquivalentTo([OrderStatus.InProgress]);
    }

    [Fact]
    public async Task An_order_past_its_due_date_is_overdue_and_still_editable()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, dueInDays: 1);
        await Database.SeedAsync(order);
        Database.Time.Advance(TimeSpan.FromDays(2));
        using var client = Database.Factory.CreateClient();

        // Act
        var body = await client.GetFromJsonAsync<MaintenanceOrderResponse>(
            $"/api/orders/{order.Id}", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        body!.IsOverdue.Should().BeTrue();
        body.IsEditable.Should().BeTrue();
    }

    [Fact]
    public async Task Getting_an_unknown_order_returns_a_not_found_problem()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var id = Guid.CreateVersion7();

        // Act
        var response = await client.GetAsync($"/api/orders/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Detail.Should().Be($"No order with id '{id}'.");
    }
}
