using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.Orders.Commands;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class CorrectOrderDetailsHandler(BikeRepairShopDbContext db, IShopClock clock, ILogger<CorrectOrderDetailsHandler> logger)
{
    public async Task<MaintenanceOrderResponse> HandleAsync(CorrectOrderDetailsCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == command.Id, cancellationToken)
            ?? throw NotFoundException.Order(command.Id);

        order.CorrectDetails(command.CustomerName, command.PhoneNumber, command.Email, command.BikeBrand, command.Notes);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Corrected the details of order {OrderId}", order.Id);
        return MaintenanceOrderResponse.From(order, clock.Today);
    }
}
