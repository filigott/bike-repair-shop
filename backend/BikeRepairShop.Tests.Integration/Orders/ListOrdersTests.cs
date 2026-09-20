using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Api.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.Orders;

public class ListOrdersTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    [Theory]
    [InlineData("kari", "Kari Nordmann")]
    [InlineData("OLA@", "Ola Hansen")]
    [InlineData("8765", "Ola Hansen")]
    public async Task Search_matches_name_phone_and_email_case_insensitively(string search, string expectedCustomer)
    {
        // Arrange
        await SeedAsync(
            OrderFixtures.Place(Clock, customerName: "Kari Nordmann", phoneNumber: "+47 912 34 567", email: "kari@example.com"),
            OrderFixtures.Place(Clock, customerName: "Ola Hansen", phoneNumber: "98765432", email: "ola@post.no"));
        using var client = Database.Factory.CreateClient();

        // Act
        var page = await client.GetFromJsonAsync<PagedResponse<MaintenanceOrderSummary>>(
            $"/api/orders?search={search}", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        page!.Items.Should().ContainSingle().Which.CustomerName.Should().Be(expectedCustomer);
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Pages_split_orders_with_the_same_due_date_without_duplicates_or_gaps()
    {
        // Arrange
        await SeedAsync(
            OrderFixtures.Place(Clock, customerName: "A"),
            OrderFixtures.Place(Clock, customerName: "B"),
            OrderFixtures.Place(Clock, customerName: "C"));
        using var client = Database.Factory.CreateClient();

        // Act
        var first = await client.GetFromJsonAsync<PagedResponse<MaintenanceOrderSummary>>(
            "/api/orders?page=1&pageSize=2", Json.Options, TestContext.Current.CancellationToken);
        var second = await client.GetFromJsonAsync<PagedResponse<MaintenanceOrderSummary>>(
            "/api/orders?page=2&pageSize=2", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        first!.Items.Should().HaveCount(2);
        second!.Items.Should().ContainSingle();
        first.Items.Concat(second.Items).Select(o => o.Id).Should().OnlyHaveUniqueItems().And.HaveCount(3);
        first.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task A_page_size_over_the_cap_is_rejected()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/orders?pageSize={ListOrdersRequest.MaxPageSize + 1}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("pageSize");
    }

    private async Task SeedAsync(params MaintenanceOrder[] orders)
    {
        await using var scope = Database.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>();
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
