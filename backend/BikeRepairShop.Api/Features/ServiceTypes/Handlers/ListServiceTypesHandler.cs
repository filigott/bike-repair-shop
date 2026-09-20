using BikeRepairShop.Api.Features.ServiceTypes.Models;
using BikeRepairShop.Database;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.ServiceTypes.Handlers;

public sealed class ListServiceTypesHandler(BikeRepairShopDbContext db)
{
    public async Task<IReadOnlyList<ServiceTypeItem>> HandleAsync(CancellationToken cancellationToken)
    {
        var definitions = await db.ServiceTypes
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

        return definitions.Select(ServiceTypeItem.From).ToList();
    }
}
