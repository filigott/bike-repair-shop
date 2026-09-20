using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Tests.Unit.Fixtures;

public static class OrderFixtures
{
    public static readonly DateOnly Today = new(2026, 9, 20);

    public static MaintenanceOrder Placed(
        string customerName = "Kari Nordmann",
        string phoneNumber = "+47 912 34 567",
        string email = "kari@example.com",
        string bikeBrand = "Trek",
        ServiceType serviceType = ServiceType.BrakeMaintenance,
        DateOnly? dueDate = null,
        string? notes = "Squeaks when braking",
        DateOnly? today = null)
    {
        return MaintenanceOrder.Place(
            customerName,
            PhoneNumber.Parse(phoneNumber),
            EmailAddress.Parse(email),
            bikeBrand,
            serviceType,
            dueDate ?? Today.AddDays(3),
            notes,
            today ?? Today);
    }

    public static MaintenanceOrder InProgress()
    {
        var order = Placed();
        order.Start();
        return order;
    }

    public static MaintenanceOrder Completed()
    {
        var order = InProgress();
        order.Complete();
        return order;
    }

    public static MaintenanceOrder Cancelled()
    {
        var order = Placed();
        order.Cancel();
        return order;
    }
}
