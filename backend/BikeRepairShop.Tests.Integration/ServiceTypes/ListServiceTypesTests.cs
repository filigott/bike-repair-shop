using System.Net.Http.Json;
using AwesomeAssertions;
using BikeRepairShop.Api.Features.ServiceTypes.Models;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Integration.Fixtures;

namespace BikeRepairShop.Tests.Integration.ServiceTypes;

public class ListServiceTypesTests(DatabaseFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task The_catalogue_is_returned_in_sort_order_with_display_names()
    {
        // Arrange
        using var client = Database.Factory.CreateClient();

        // Act
        var items = await client.GetFromJsonAsync<List<ServiceTypeItem>>("/api/service-types", Json.Options, TestContext.Current.CancellationToken);

        // Assert
        items.Should().Equal(
            new ServiceTypeItem(ServiceType.WheelAdjustment, "Wheel adjustment"),
            new ServiceTypeItem(ServiceType.ChainReplacement, "Chain replacement"),
            new ServiceTypeItem(ServiceType.BrakeMaintenance, "Brake maintenance"));
    }
}
