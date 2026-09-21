using BikeRepairShop.Api.Features.DemoData.Models;
using BikeRepairShop.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BikeRepairShop.Api.Features.DemoData.Handlers;

public sealed class GetDemoDataStatusHandler(BikeRepairShopDbContext context, IOptions<DemoDataOptions> options)
{
    public async Task<DemoDataStatus> HandleAsync(CancellationToken cancellationToken)
    {
        return DemoDataStatus.From(
            options.Value.Enabled,
            await context.Orders.CountAsync(cancellationToken));
    }
}
