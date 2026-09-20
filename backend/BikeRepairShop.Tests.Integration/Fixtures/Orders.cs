using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public static class Orders
{
    public static MaintenanceOrder Place(
        IShopClock clock,
        string customerName = "Kari Nordmann",
        string phoneNumber = "+47 912 34 567",
        string email = "kari@example.com",
        string bikeBrand = "Trek",
        ServiceType serviceType = ServiceType.BrakeMaintenance,
        int dueInDays = 3,
        string? notes = "Squeaks when braking")
    {
        return MaintenanceOrder.Place(
            customerName,
            PhoneNumber.Parse(phoneNumber),
            EmailAddress.Parse(email),
            bikeBrand,
            serviceType,
            clock.Today.AddDays(dueInDays),
            notes,
            clock);
    }
}
