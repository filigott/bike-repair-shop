using AwesomeAssertions;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeRepairShop.Tests.Integration.Persistence;

public class OrderPersistenceTests(DatabaseFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task A_saved_order_reloads_with_its_status_value_objects_and_stamp()
    {
        // Arrange
        var clock = Database.Factory.Services.GetRequiredService<IShopClock>();
        var order = Orders.Place(clock, email: "Kari.Nordmann@Example.com", phoneNumber: "+47 912 34 567");
        order.Start();
        await using (var scope = Database.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>();
            db.Orders.Add(order);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        await using var readScope = Database.CreateScope();
        var reloaded = await readScope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>()
            .Orders.SingleAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);

        // Assert
        reloaded.Status.Should().Be(OrderStatus.InProgress);
        reloaded.Email.Should().Be(EmailAddress.Parse("kari.nordmann@example.com"));
        reloaded.PhoneNumber.Should().Be(PhoneNumber.Parse("+4791234567"));
        reloaded.ServiceType.Should().Be(ServiceType.BrakeMaintenance);
        reloaded.DueDate.Should().Be(order.DueDate);
        reloaded.Notes.Should().Be(order.Notes);
        reloaded.PlacedAt.Should().BeCloseTo(order.PlacedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task The_service_catalogue_is_seeded_in_sort_order()
    {
        // Arrange
        await using var scope = Database.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>();

        // Act
        var codes = await db.ServiceTypes
            .OrderBy(s => s.SortOrder)
            .Select(s => s.Code)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        codes.Should().Equal(ServiceType.WheelAdjustment, ServiceType.ChainReplacement, ServiceType.BrakeMaintenance);
    }
}
