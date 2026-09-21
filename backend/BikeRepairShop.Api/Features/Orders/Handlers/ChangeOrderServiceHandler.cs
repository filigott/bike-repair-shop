using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class ChangeOrderServiceHandler(BikeRepairShopDbContext db, IShopClock clock, ILogger<ChangeOrderServiceHandler> logger)
{
    public async Task<MaintenanceOrderResponse> HandleAsync(ChangeOrderServiceCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == command.Id, cancellationToken)
            ?? throw NotFoundException.Order(command.Id);

        order.ChangeService(command.ServiceType);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Changed order {OrderId} to {ServiceType}", order.Id, order.ServiceType);
        return MaintenanceOrderResponse.From(order, clock.Today);
    }
}
