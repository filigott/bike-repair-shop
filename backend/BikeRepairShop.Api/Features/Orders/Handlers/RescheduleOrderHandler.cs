using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class RescheduleOrderHandler(BikeRepairShopDbContext db, IShopClock clock, ILogger<RescheduleOrderHandler> logger)
{
    public async Task<MaintenanceOrderResponse> HandleAsync(RescheduleOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == command.Id, cancellationToken)
            ?? throw NotFoundException.Order(command.Id);

        if (order.Version != command.Version)
        {
            throw ConflictException.OrderChanged();
        }

        order.Reschedule(command.DueDate, clock);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Rescheduled order {OrderId} to {DueDate}", order.Id, order.DueDate);
        return MaintenanceOrderResponse.From(order, clock.Today);
    }
}
