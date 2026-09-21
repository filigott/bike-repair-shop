using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.DemoData.Handlers;

public sealed class ResetDemoDataHandler(BikeRepairShopDbContext context, IShopClock clock)
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        await context.Orders.ExecuteDeleteAsync(cancellationToken);

        var orders = DemoOrders.Build(clock);
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync(cancellationToken);

        return orders.Count;
    }
}
