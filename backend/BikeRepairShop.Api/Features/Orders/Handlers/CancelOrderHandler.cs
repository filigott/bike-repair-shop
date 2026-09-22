using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class CancelOrderHandler(BikeRepairShopDbContext db, IShopClock clock, ILogger<CancelOrderHandler> logger)
{
    public async Task<MaintenanceOrderResponse> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == command.Id, cancellationToken)
            ?? throw NotFoundException.Order(command.Id);

        order.Cancel();
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderNumber} moved to {Status}", order.OrderNumber, order.Status);
        return MaintenanceOrderResponse.From(order, clock.Today);
    }
}
