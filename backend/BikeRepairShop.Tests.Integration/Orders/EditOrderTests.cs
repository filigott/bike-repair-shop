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

public class EditOrderTests(DatabaseFixture database) : IntegrationTest(database)
{
    private IShopClock Clock => Database.Factory.Services.GetRequiredService<IShopClock>();

    #region Details

    [Fact]
    public async Task Correcting_the_details_persists_them_and_leaves_the_service_and_date_alone()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, serviceType: ServiceType.ChainReplacement, dueInDays: 4);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();
        var request = new CorrectOrderDetailsRequest("Ola Hansen", "98765432", "Ola@Post.no", "Bianchi", "Also check the gears", order.Version);

        // Act
        var response = await client.PutAsJsonAsync($"/api/orders/{order.Id}/details", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var saved = await ReloadAsync(order.Id);
        saved.CustomerName.Should().Be("Ola Hansen");
        saved.PhoneNumber.Value.Should().Be("98765432");
        saved.Email.Value.Should().Be("ola@post.no");
        saved.BikeBrand.Should().Be("Bianchi");
        saved.Notes.Should().Be("Also check the gears");
        saved.ServiceType.Should().Be(ServiceType.ChainReplacement);
        saved.DueDate.Should().Be(order.DueDate);
    }

    [Fact]
    public async Task Correcting_the_details_of_an_overdue_order_is_allowed()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, dueInDays: 1);
        await Database.SeedAsync(order);
        Database.Time.Advance(TimeSpan.FromDays(3));
        using var client = Database.Factory.CreateClient();
        var request = new CorrectOrderDetailsRequest("Kari Nordmann", "+4791234567", "kari@example.com", "Trek", null, order.Version);

        // Act
        var response = await client.PutAsJsonAsync($"/api/orders/{order.Id}/details", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken);
        body!.IsOverdue.Should().BeTrue();
        (await ReloadAsync(order.Id)).Notes.Should().BeNull();
    }

    [Fact]
    public async Task Correcting_the_details_with_a_malformed_phone_number_reports_the_field()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();
        var request = new CorrectOrderDetailsRequest("Kari Nordmann", "12", "kari@example.com", "Trek", null, order.Version);

        // Act
        var response = await client.PutAsJsonAsync($"/api/orders/{order.Id}/details", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("phoneNumber");
    }

    [Fact]
    public async Task Correcting_the_details_of_a_cancelled_order_is_refused()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        order.Cancel();
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();
        var request = new CorrectOrderDetailsRequest("Ola Hansen", "98765432", "ola@post.no", "Bianchi", null, order.Version);

        // Act
        var response = await client.PutAsJsonAsync($"/api/orders/{order.Id}/details", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Detail.Should().Be("A Cancelled order cannot be changed.");
        (await ReloadAsync(order.Id)).CustomerName.Should().Be("Kari Nordmann");
    }

    [Fact]
    public async Task Correcting_the_details_of_an_unknown_order_returns_a_not_found_problem()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();
        var id = Guid.CreateVersion7();
        var request = new CorrectOrderDetailsRequest("Ola Hansen", "98765432", "ola@post.no", "Bianchi", null, 1);

        // Act
        var response = await client.PutAsJsonAsync($"/api/orders/{id}/details", request, Json.Options, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Service

    [Fact]
    public async Task Changing_the_service_persists_the_new_one()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, serviceType: ServiceType.BrakeMaintenance);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/service",
            new ChangeOrderServiceRequest(ServiceType.WheelAdjustment, order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReloadAsync(order.Id)).ServiceType.Should().Be(ServiceType.WheelAdjustment);
    }

    [Fact]
    public async Task Changing_the_service_to_an_unknown_one_reports_the_field()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/service",
            new ChangeOrderServiceRequest((ServiceType)99, order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("serviceType");
    }

    #endregion

    #region Due date

    [Fact]
    public async Task Rescheduling_persists_the_new_due_date()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, dueInDays: 2);
        await Database.SeedAsync(order);
        var moved = Clock.Today.AddDays(9);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/due-date",
            new RescheduleOrderRequest(moved, order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReloadAsync(order.Id)).DueDate.Should().Be(moved);
    }

    [Fact]
    public async Task Rescheduling_into_the_past_is_refused_and_leaves_the_date_alone()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, dueInDays: 2);
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
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Detail.Should().Be("The due date cannot be before today.");
        (await ReloadAsync(order.Id)).DueDate.Should().Be(order.DueDate);
    }

    [Fact]
    public async Task Rescheduling_without_a_date_reports_the_field_rather_than_the_binder()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/due-date",
            new RescheduleOrderRequest(null, order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("dueDate");
    }

    #endregion

    #region Concurrency

    [Fact]
    public async Task Editing_with_a_stale_version_is_refused_and_keeps_the_newer_write()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, dueInDays: 2);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();
        var first = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/details",
            new CorrectOrderDetailsRequest("Ola Hansen", "98765432", "ola@post.no", "Bianchi", null, order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/due-date",
            new RescheduleOrderRequest(Clock.Today.AddDays(9), order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var saved = await ReloadAsync(order.Id);
        saved.CustomerName.Should().Be("Ola Hansen");
        saved.DueDate.Should().Be(order.DueDate);
    }

    [Fact]
    public async Task The_version_an_edit_returns_is_accepted_by_the_next_edit()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock, serviceType: ServiceType.BrakeMaintenance);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();
        var first = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/details",
            new CorrectOrderDetailsRequest("Ola Hansen", "98765432", "ola@post.no", "Bianchi", null, order.Version),
            Json.Options,
            TestContext.Current.CancellationToken);
        var edited = await first.Content.ReadFromJsonAsync<MaintenanceOrderResponse>(Json.Options, TestContext.Current.CancellationToken);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/service",
            new ChangeOrderServiceRequest(ServiceType.WheelAdjustment, edited!.Version),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        edited.Version.Should().NotBe(order.Version);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReloadAsync(order.Id)).ServiceType.Should().Be(ServiceType.WheelAdjustment);
    }

    [Fact]
    public async Task Editing_without_a_version_reports_the_field()
    {
        // Arrange
        var order = OrderFixtures.Place(Clock);
        await Database.SeedAsync(order);
        using var client = Database.Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/due-date",
            new RescheduleOrderRequest(Clock.Today.AddDays(5), null),
            Json.Options,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json.Options, TestContext.Current.CancellationToken);
        problem!.Errors.Keys.Should().BeEquivalentTo("version");
    }

    #endregion

    private async Task<MaintenanceOrder> ReloadAsync(Guid id)
    {
        await using var scope = Database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken);
    }
}
