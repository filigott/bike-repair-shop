using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Features.DemoData.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BikeRepairShop.Api.Features.DemoData.Handlers;

public sealed class ResetDemoDataHandler(
    BikeRepairShopDbContext context,
    IShopClock clock,
    IOptions<DemoDataOptions> options)
{
    public async Task<DemoDataStatus> HandleAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            throw NotFoundException.DemoDataDisabled();
        }

        context.Orders.RemoveRange(await context.Orders.ToListAsync(cancellationToken));

        var orders = DemoOrders.Build(clock);
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync(cancellationToken);

        return DemoDataStatus.From(true, orders.Count);
    }
}
