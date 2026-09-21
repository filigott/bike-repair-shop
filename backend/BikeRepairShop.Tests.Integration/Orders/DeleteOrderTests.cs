using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.Orders;

public class DeleteOrderTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    [Fact]
    public async Task Deleting_an_order_removes_the_row_and_leaves_the_others()
    {
        // Arrange
        var doomed = OrderFixtures.Place(Clock, customerName: "Kari Nordmann");
        var kept = OrderFixtures.Place(Clock, customerName: "Ola Hansen");
        await Database.SeedAsync(doomed, kept);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/orders/{doomed.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var remaining = await LoadIdsAsync();
        remaining.Should().Equal(kept.Id);
    }

    [Fact]
    public async Task Deleting_a_completed_order_is_allowed()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Complete();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/orders/{order.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await LoadIdsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Deleting_an_unknown_order_returns_a_not_found_problem()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var id = Guid.CreateVersion7();

        // Act
        var response = await client.DeleteAsync($"/api/orders/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Detail.Should().Be($"No order with id '{id}'.");
    }

    private async Task<List<Guid>> LoadIdsAsync()
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.Select(o => o.Id).ToListAsync(TestContext.Current.CancellationToken);
    }
}
