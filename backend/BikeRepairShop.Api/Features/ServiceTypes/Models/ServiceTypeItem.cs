using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.ServiceTypes.Models;

public sealed record ServiceTypeItem(ServiceType Code, string DisplayName)
{
    public static ServiceTypeItem From(ServiceTypeDefinition definition)
    {
        return new ServiceTypeItem(definition.Code, definition.DisplayName);
    }
}
