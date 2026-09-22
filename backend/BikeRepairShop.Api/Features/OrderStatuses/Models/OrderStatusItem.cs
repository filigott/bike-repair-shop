using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.OrderStatuses.Models;

public sealed record OrderStatusItem(
    OrderStatus Status,
    IReadOnlyList<OrderStatus> AllowedTransitions)
{
    public static OrderStatusItem From(OrderStatus status)
    {
        return new OrderStatusItem(status, status.AllowedTransitions);
    }
}
