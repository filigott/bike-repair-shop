using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.DemoData.Handlers;

public sealed class SeedDemoDataIfEmptyHandler(BikeRepairShopDbContext context, IShopClock clock)
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        if (await context.Orders.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var orders = DemoOrders.Build(clock);
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync(cancellationToken);

        return orders.Count;
    }
}
