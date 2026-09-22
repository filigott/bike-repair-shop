using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record MaintenanceOrderResponse(
    Guid Id,
    long OrderNumber,
    string CustomerName,
    string PhoneNumber,
    string Email,
    string BikeBrand,
    ServiceType ServiceType,
    DateOnly DueDate,
    string? Notes,
    OrderStatus Status,
    DateTimeOffset PlacedAt,
    bool IsOverdue,
    bool IsEditable,
    IReadOnlyList<OrderAction> AllowedActions)
{
    public static MaintenanceOrderResponse From(MaintenanceOrder order, DateOnly today)
    {
        return new MaintenanceOrderResponse(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.PhoneNumber.Value,
            order.Email.Value,
            order.BikeBrand,
            order.ServiceType,
            order.DueDate,
            order.Notes,
            order.Status,
            order.PlacedAt,
            order.IsOverdueOn(today),
            order.Status.IsEditable,
            order.AllowedActions);
    }
}
