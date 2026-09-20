using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record MaintenanceOrderSummary(
    Guid Id,
    string CustomerName,
    string PhoneNumber,
    string Email,
    string BikeBrand,
    ServiceType ServiceType,
    DateOnly DueDate,
    OrderStatus Status,
    bool IsOverdue)
{
    public static MaintenanceOrderSummary From(MaintenanceOrder order, DateOnly today)
    {
        return new MaintenanceOrderSummary(
            order.Id,
            order.CustomerName,
            order.PhoneNumber.Value,
            order.Email.Value,
            order.BikeBrand,
            order.ServiceType,
            order.DueDate,
            order.Status,
            order.IsOverdueOn(today));
    }
}
