using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.DemoData;

public static class DemoOrders
{
    private sealed record Spec(
        string CustomerName,
        string PhoneNumber,
        string Email,
        string BikeBrand,
        ServiceType ServiceType,
        int PlacedDaysAgo,
        int DueDaysFromToday,
        OrderStatus Status,
        string? Notes = null);

    public static IReadOnlyList<MaintenanceOrder> Build(IShopClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return [.. Specs.Select(spec => Place(spec, clock))];
    }

    private static MaintenanceOrder Place(Spec spec, IShopClock clock)
    {
        var placedOn = clock.Today.AddDays(-spec.PlacedDaysAgo);
        var order = MaintenanceOrder.Place(
            spec.CustomerName,
            PhoneNumber.Parse(spec.PhoneNumber),
            EmailAddress.Parse(spec.Email),
            spec.BikeBrand,
            spec.ServiceType,
            clock.Today.AddDays(spec.DueDaysFromToday),
            spec.Notes,
            new SeedClock(clock.Now.AddDays(-spec.PlacedDaysAgo), placedOn));

        switch (spec.Status)
        {
            case OrderStatus.InProgress:
                order.Start();
                break;
            case OrderStatus.Completed:
                order.Start();
                order.Complete();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        return order;
    }

    private static readonly Spec[] Specs =
    [
        new("Ola Nordmann", "+4751223344", "ola.nordmann@example.no", "Trek", ServiceType.WheelAdjustment, 2, 5, OrderStatus.Received, "Front wheel pulls to the left."),
        new("Kari Bakke", "+4751224455", "kari.bakke@example.no", "Specialized", ServiceType.ChainReplacement, 1, 3, OrderStatus.Received),
        new("Jonas Tveit", "+4791002233", "jonas.tveit@example.no", "Giant", ServiceType.BrakeMaintenance, 4, 9, OrderStatus.InProgress, "Rear brake squeals under load."),
        new("Ingrid Solheim", "+4791003344", "ingrid.solheim@example.no", "Cannondale", ServiceType.WheelAdjustment, 0, 12, OrderStatus.Received),
        new("Magnus Vik", "+4740551122", "magnus.vik@example.no", "Bianchi", ServiceType.ChainReplacement, 3, 2, OrderStatus.InProgress),
        new("Silje Haugen", "+4740552233", "silje.haugen@example.no", "Scott", ServiceType.BrakeMaintenance, 6, 7, OrderStatus.Received, "Bring the spare pads from the back shelf."),
        new("Anders Lie", "+4746003311", "anders.lie@example.no", "Merida", ServiceType.WheelAdjustment, 1, 1, OrderStatus.Received),
        new("Marte Rygg", "+4746004422", "marte.rygg@example.no", "Cube", ServiceType.ChainReplacement, 8, 14, OrderStatus.InProgress),
        new("Sondre Aas", "+4798110022", "sondre.aas@example.no", "Canyon", ServiceType.BrakeMaintenance, 2, 6, OrderStatus.Received),
        new("Elise Moen", "+4798112233", "elise.moen@example.no", "DBS", ServiceType.WheelAdjustment, 5, 10, OrderStatus.Received, "Customer is away until Thursday."),
        new("Henrik Berge", "+4745220011", "henrik.berge@example.no", "Nakamura", ServiceType.ChainReplacement, 0, 4, OrderStatus.Received),
        new("Nora Eide", "+4745221122", "nora.eide@example.no", "Trek", ServiceType.BrakeMaintenance, 7, 8, OrderStatus.InProgress),
        new("Oskar Fjell", "+4792330044", "oskar.fjell@example.no", "Specialized", ServiceType.WheelAdjustment, 3, 15, OrderStatus.Received),
        new("Thea Lunde", "+4792331155", "thea.lunde@example.no", "Giant", ServiceType.ChainReplacement, 9, 11, OrderStatus.Received),
        new("Even Dahl", "+4741660022", "even.dahl@example.no", "Cannondale", ServiceType.BrakeMaintenance, 1, 20, OrderStatus.Received, "Full service booked for the spring."),
        new("Amalie Strand", "+4741661133", "amalie.strand@example.no", "Bianchi", ServiceType.WheelAdjustment, 4, 13, OrderStatus.InProgress),
        new("Kristian Holm", "+4799440011", "kristian.holm@example.no", "Scott", ServiceType.ChainReplacement, 2, 18, OrderStatus.Received),
        new("Sara Nilsen", "+4799441122", "sara.nilsen@example.no", "Merida", ServiceType.BrakeMaintenance, 11, 16, OrderStatus.Received),
        new("Mathias Ruud", "+4747880033", "mathias.ruud@example.no", "Cube", ServiceType.WheelAdjustment, 6, 21, OrderStatus.Received),
        new("Vilde Sande", "+4747881144", "vilde.sande@example.no", "Canyon", ServiceType.ChainReplacement, 0, 25, OrderStatus.InProgress),
        new("Bjørn Kvam", "+4793220055", "bjorn.kvam@example.no", "DBS", ServiceType.BrakeMaintenance, 14, -3, OrderStatus.Received, "Customer has not answered two calls."),
        new("Hanne Foss", "+4793221166", "hanne.foss@example.no", "Nakamura", ServiceType.WheelAdjustment, 21, -8, OrderStatus.InProgress, "Waiting on a spoke set that never arrived."),
        new("Lars Åsen", "+4744550077", "lars.aasen@example.no", "Trek", ServiceType.ChainReplacement, 16, -1, OrderStatus.Completed),
        new("Ida Rein", "+4744551188", "ida.rein@example.no", "Specialized", ServiceType.BrakeMaintenance, 12, 2, OrderStatus.Completed, "Picked up the same afternoon."),
        new("Petter Nes", "+4790660099", "petter.nes@example.no", "Giant", ServiceType.WheelAdjustment, 10, 5, OrderStatus.Cancelled, "Customer bought a new bike instead."),
        new("Live Storm", "+4790661100", "live.storm@example.no", "Cannondale", ServiceType.ChainReplacement, 18, -4, OrderStatus.Cancelled),
    ];
}
