using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public static class PlaceOrderRequests
{
    public static PlaceOrderRequest Valid(IShopClock clock, int dueInDays = 3)
    {
        return new PlaceOrderRequest(
            "Kari Nordmann",
            "+47 912 34 567",
            "Kari@Example.com",
            "Trek",
            ServiceType.BrakeMaintenance,
            clock.Today.AddDays(dueInDays),
            "Squeaks when braking");
    }
}
