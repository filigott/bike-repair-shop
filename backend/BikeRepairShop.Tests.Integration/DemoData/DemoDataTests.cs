using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.DemoData;
using BikeRepairShop.Api.Features.DemoData.Handlers;
using BikeRepairShop.Api.Features.DemoData.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.DemoData;

public class DemoDataTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    #region Seeding

    [Fact]
    public async Task Seeding_an_empty_database_places_the_whole_demo_set()
    {
        // Act
        var seeded = await SeedIfEmptyAsync();

        // Assert
        seeded.Should().Be(DemoOrders.Build(Clock).Count);
        (await CountOrdersAsync()).Should().Be(seeded);
    }

    [Fact]
    public async Task Seeding_leaves_a_database_that_already_holds_an_order_untouched()
    {
        // Arrange
        await Database.SeedAsync(OrderFixtures.Place(Clock, customerName: "Placed by hand"));

        // Act
        var seeded = await SeedIfEmptyAsync();

        // Assert
        seeded.Should().Be(0);
        (await LoadOrdersAsync()).Should().ContainSingle().Which.CustomerName.Should().Be("Placed by hand");
    }

    [Fact]
    public async Task The_demo_set_carries_the_states_the_smoke_test_cannot_otherwise_reach()
    {
        // Arrange
        await SeedIfEmptyAsync();
        var today = Clock.Today;

        // Act
        var orders = await LoadOrdersAsync();

        // Assert
        orders.Should().Contain(order => order.IsOverdueOn(today));
        orders.Should().Contain(order => order.Status == OrderStatus.Completed);
        orders.Should().Contain(order => order.Status == OrderStatus.Cancelled);
        orders.Count.Should().BeGreaterThan(20, "paging at a page size of 20 needs a second page");
    }

    [Fact]
    public async Task Every_service_the_shop_offers_appears_in_the_demo_set()
    {
        // Arrange
        await SeedIfEmptyAsync();

        // Act
        var services = (await LoadOrdersAsync()).Select(order => order.ServiceType).Distinct();

        // Assert
        services.Should().BeEquivalentTo(Enum.GetValues<ServiceType>());
    }

    #endregion

    #region Reset

    [Fact]
    public async Task Resetting_replaces_whatever_the_database_already_held()
    {
        // Arrange
        await Database.SeedAsync(OrderFixtures.Place(Clock, customerName: "Placed by hand"));

        // Act
        await using (var scope = Database.CreateScope())
        {
            await DemoDataFixtures.SwitchedOnReset(scope, Clock)
                .HandleAsync(TestContext.Current.CancellationToken);
        }

        // Assert
        var orders = await LoadOrdersAsync();
        orders.Should().NotContain(order => order.CustomerName == "Placed by hand");
        orders.Count.Should().Be(DemoOrders.Build(Clock).Count);
    }

    [Fact]
    public async Task A_reset_that_fails_after_clearing_the_table_keeps_the_orders_it_found()
    {
        // Arrange
        await Database.SeedAsync(OrderFixtures.Place(Clock, customerName: "Placed by hand"));
        using var cancellation = new CancellationTokenSource();
        await using var scope = Database.CreateScope();
        var handler = DemoDataFixtures.SwitchedOnReset(scope, new CancellingShopClock(Clock, cancellation));

        // Act
        var reset = () => handler.HandleAsync(cancellation.Token);

        // Assert
        await reset.Should().ThrowAsync<OperationCanceledException>();
        (await LoadOrdersAsync()).Should().ContainSingle(order => order.CustomerName == "Placed by hand");
    }

    #endregion

    #region The gate

    [Fact]
    public async Task The_status_endpoint_reports_the_feature_as_off_unless_it_is_switched_on()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var status = await client.GetFromJsonAsync<DemoDataStatus>(
            "/api/demo-data", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        status!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Resetting_is_not_offered_at_all_when_the_feature_is_off()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/demo-data/reset", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        problem.GetProperty("code").GetString().Should().Be("DemoDataDisabled");
    }

    [Fact]
    public async Task Switching_the_feature_on_seeds_the_database_as_the_host_starts()
    {
        // Arrange
        using var factory = Database.Factory.WithWebHostBuilder(
            builder => builder.UseSetting($"{DemoDataOptions.SectionName}:Enabled", "true"));

        // Act
        using var client = factory.CreateClient();
        var status = await client.GetFromJsonAsync<DemoDataStatus>(
            "/api/demo-data", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        status!.Enabled.Should().BeTrue();
        status.OrderCount.Should().Be(DemoOrders.Build(Clock).Count);
    }

    #endregion

    private async Task<int> SeedIfEmptyAsync()
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<SeedDemoDataIfEmptyHandler>()
            .HandleAsync(TestContext.Current.CancellationToken);
    }

    private async Task<List<MaintenanceOrder>> LoadOrdersAsync()
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> CountOrdersAsync()
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.CountAsync(TestContext.Current.CancellationToken);
    }
}
