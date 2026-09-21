using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Database;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class DeleteOrderHandler(BikeRepairShopDbContext db, ILogger<DeleteOrderHandler> logger)
{
    public async Task HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var deleted = await db.Orders
            .Where(o => o.Id == command.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            throw NotFoundException.Order(command.Id);
        }

        logger.LogInformation("Deleted order {OrderId}", command.Id);
    }
}
