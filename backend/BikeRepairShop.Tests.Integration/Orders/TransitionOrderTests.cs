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
        body.AllowedTransitions.Should().BeEquivalentTo([OrderStatus.Completed, OrderStatus.Cancelled]);
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
        problem!.Detail.Should().Be("An order cannot move from Completed to Cancelled.");
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

    #endregion

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid id, string command)
    {
        return client.PostAsync($"/api/orders/{id}/{command}", null, TestContext.Current.CancellationToken);
    }

    private async Task<MaintenanceOrder> ReloadAsync(Guid id)
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken);
    }
}
