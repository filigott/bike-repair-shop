using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record MaintenanceOrderResponse(
    Guid Id,
    string CustomerName,
    string PhoneNumber,
    string Email,
    string BikeBrand,
    ServiceType ServiceType,
    DateOnly DueDate,
    string? Notes,
    OrderStatus Status,
    DateTimeOffset PlacedAt,
    IReadOnlyList<OrderStatus> AllowedTransitions)
{
    public static MaintenanceOrderResponse From(MaintenanceOrder order)
    {
        return new MaintenanceOrderResponse(
            order.Id,
            order.CustomerName,
            order.PhoneNumber.Value,
            order.Email.Value,
            order.BikeBrand,
            order.ServiceType,
            order.DueDate,
            order.Notes,
            order.Status,
            order.PlacedAt,
            order.AllowedTransitions);
    }
}
