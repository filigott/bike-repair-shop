using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.OrderStatuses.Models;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;

namespace BikeRepairShop.Tests.Integration.OrderStatuses;

public class ListOrderStatusesTests(DatabaseFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task The_catalogue_describes_every_status_the_domain_declares()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var items = await ListAsync(client);

        // Assert
        items.Select(item => item.Status).Should().Equal(Enum.GetValues<OrderStatus>());
    }

    [Fact]
    public async Task Each_status_carries_the_targets_the_domain_gives_it()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var items = await ListAsync(client);

        // Assert
        items.Should().AllSatisfy(item =>
            item.AllowedTransitions.Should().Equal(item.Status.AllowedTransitions));
    }

    [Fact]
    public async Task A_cancelled_order_is_shown_as_the_end_of_the_line()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var items = await ListAsync(client);

        // Assert
        var cancelled = items.Single(item => item.Status == OrderStatus.Cancelled);
        cancelled.AllowedTransitions.Should().BeEmpty();
    }

    private static async Task<List<OrderStatusItem>> ListAsync(HttpClient client)
    {
        var items = await client.GetFromJsonAsync<List<OrderStatusItem>>(
            "/api/order-statuses", Json.Options, TestContext.Current.CancellationToken);

        return items.Should().NotBeNull().And.Subject.As<List<OrderStatusItem>>();
    }
}
