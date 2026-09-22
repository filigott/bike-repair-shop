using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class PlaceOrderHandler(BikeRepairShopDbContext db, IShopClock clock, ILogger<PlaceOrderHandler> logger)
{
    public async Task<MaintenanceOrderResponse> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var order = MaintenanceOrder.Place(
            command.CustomerName,
            command.PhoneNumber,
            command.Email,
            command.BikeBrand,
            command.ServiceType,
            command.DueDate,
            command.Notes,
            clock);

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Placed order {OrderNumber} ({OrderId}) for {ServiceType} due {DueDate}",
            order.OrderNumber,
            order.Id,
            order.ServiceType,
            order.DueDate);
        return MaintenanceOrderResponse.From(order, clock.Today);
    }
}
