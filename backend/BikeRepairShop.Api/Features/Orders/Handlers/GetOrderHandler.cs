using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Api.Features.Orders.Queries;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class GetOrderHandler(BikeRepairShopDbContext db, IShopClock clock)
{
    public async Task<MaintenanceOrderResponse> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == query.Id, cancellationToken)
            ?? throw NotFoundException.Order(query.Id);

        return MaintenanceOrderResponse.From(order, clock.Today);
    }
}
