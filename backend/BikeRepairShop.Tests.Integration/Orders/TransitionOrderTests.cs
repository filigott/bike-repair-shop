using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.Orders;

public class TransitionOrderTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    #region Legal moves

    [Fact]
    public async Task Starting_a_received_order_persists_it_as_in_progress()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, "start");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken);
        body!.Status.Should().Be(OrderStatus.InProgress);
        body.AllowedActions.Should().BeEquivalentTo([OrderAction.Complete, OrderAction.Cancel]);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public async Task Completing_an_order_in_progress_persists_it_as_completed()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Start();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, "complete");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Cancelling_a_received_order_persists_it_as_cancelled()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, "cancel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Reopening_a_completed_order_persists_it_as_in_progress()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Complete();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, "reopen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.InProgress);
    }


    [Fact]
    public async Task An_order_runs_from_placed_to_completed_over_the_api_alone()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var placed = await client.PostAsJsonAsync("/api/orders", PlaceOrderRequests.Valid(Clock), Json.Options, TestContext.Current.CancellationToken);
        var order = (await placed.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken))!;

        // Act
        var started = await ReadAsync(await PostAsync(client, order.Id, "start"));
        var completed = await ReadAsync(await PostAsync(client, order.Id, "complete"));

        // Assert
        order.AllowedActions.Should().BeEquivalentTo([OrderAction.Start, OrderAction.Complete, OrderAction.Cancel]);
        started.AllowedActions.Should().BeEquivalentTo([OrderAction.Complete, OrderAction.Cancel]);
        completed.AllowedActions.Should().BeEquivalentTo([OrderAction.Reopen]);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Completing_an_order_stops_reporting_it_as_editable()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var body = await ReadAsync(await PostAsync(client, order.Id, "complete"));

        // Assert
        body.IsEditable.Should().BeFalse();
    }

    #endregion

    #region Refusals

    [Fact]
    public async Task Cancelling_a_completed_order_is_a_conflict_and_leaves_it_completed()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Complete();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, "cancel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Detail.Should().Be("The Cancel action is not allowed on a Completed order.");
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Starting_a_completed_order_is_a_conflict_because_only_reopen_leaves_it()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Complete();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, "start");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Transitioning_an_unknown_order_is_a_not_found()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, Guid.CreateVersion7(), "start");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Theory]
    [InlineData("start")]
    [InlineData("complete")]
    [InlineData("reopen")]
    public async Task A_cancelled_order_refuses_every_move(string command)
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Cancel();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await PostAsync(client, order.Id, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await ReloadAsync(order.Id)).Status.Should().Be(OrderStatus.Cancelled);
    }

    #endregion

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid id, string command)
    {
        return client.PostAsync($"/api/orders/{id}/{command}", null, TestContext.Current.CancellationToken);
    }

    private static async Task<MaintenanceOrderResponse> ReadAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken))!;
    }

    private async Task<MaintenanceOrder> ReloadAsync(Guid id)
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken);
    }
}
