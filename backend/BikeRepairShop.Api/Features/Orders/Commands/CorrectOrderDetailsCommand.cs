using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Commands;

public sealed record CorrectOrderDetailsCommand(
    Guid Id,
    string CustomerName,
    PhoneNumber PhoneNumber,
    EmailAddress Email,
    string BikeBrand,
    string? Notes)
{
    public static CorrectOrderDetailsCommand From(Guid id, CorrectOrderDetailsRequest request)
    {
        return new CorrectOrderDetailsCommand(
            id,
            request.CustomerName,
            PhoneNumber.Parse(request.PhoneNumber),
            EmailAddress.Parse(request.Email),
            request.BikeBrand,
            request.Notes);
    }
}
