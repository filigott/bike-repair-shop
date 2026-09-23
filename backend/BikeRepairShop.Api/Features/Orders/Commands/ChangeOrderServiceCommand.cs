using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Commands;

public sealed record ChangeOrderServiceCommand(Guid Id, ServiceType ServiceType, uint Version)
{
    public static ChangeOrderServiceCommand From(Guid id, ChangeOrderServiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Version);

        return new ChangeOrderServiceCommand(id, request.ServiceType, request.Version.Value);
    }
}
