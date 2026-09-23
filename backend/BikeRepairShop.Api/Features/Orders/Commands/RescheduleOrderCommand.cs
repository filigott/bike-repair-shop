using BikeRepairShop.Api.Features.Orders.Models;

namespace BikeRepairShop.Api.Features.Orders.Commands;

public sealed record RescheduleOrderCommand(Guid Id, DateOnly DueDate, uint Version)
{
    public static RescheduleOrderCommand From(Guid id, RescheduleOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.DueDate);
        ArgumentNullException.ThrowIfNull(request.Version);

        return new RescheduleOrderCommand(id, request.DueDate.Value, request.Version.Value);
    }
}
