using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Commands;

public sealed record PlaceOrderCommand(
    string CustomerName,
    PhoneNumber PhoneNumber,
    EmailAddress Email,
    string BikeBrand,
    ServiceType ServiceType,
    DateOnly DueDate,
    string? Notes)
{
    public static PlaceOrderCommand From(PlaceOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.DueDate);

        return new PlaceOrderCommand(
            request.CustomerName,
            PhoneNumber.Parse(request.PhoneNumber),
            EmailAddress.Parse(request.Email),
            request.BikeBrand,
            request.ServiceType,
            request.DueDate.Value,
            request.Notes);
    }
}
