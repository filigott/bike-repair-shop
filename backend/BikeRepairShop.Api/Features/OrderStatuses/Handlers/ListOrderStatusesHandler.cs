using BikeRepairShop.Api.Features.OrderStatuses.Models;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.OrderStatuses.Handlers;

public sealed class ListOrderStatusesHandler
{
    public IReadOnlyList<OrderStatusItem> Handle()
    {
        return Enum.GetValues<OrderStatus>().Select(OrderStatusItem.From).ToList();
    }
}
