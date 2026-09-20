using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Api.Features.Orders.Queries;
using BikeRepairShop.Api.Models;
using BikeRepairShop.Database;
using BikeRepairShop.Database.Specifications;
using BikeRepairShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Features.Orders.Handlers;

public sealed class ListOrdersHandler(BikeRepairShopDbContext db, IShopClock clock)
{
    public async Task<PagedResponse<MaintenanceOrderSummary>> HandleAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = db.Orders.AsNoTracking();
        if (query.Search is not null)
        {
            orders = orders.Where(MaintenanceOrderSpecifications.CustomerMatches(query.Search));
        }

        var totalCount = await orders.CountAsync(cancellationToken);
        var page = await orders
            .OrderBy(o => o.DueDate)
            .ThenBy(o => o.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var today = clock.Today;
        var items = page.Select(o => MaintenanceOrderSummary.From(o, today)).ToList();
        return new PagedResponse<MaintenanceOrderSummary>(items, query.Page, query.PageSize, totalCount);
    }
}
