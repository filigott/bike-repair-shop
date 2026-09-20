using BikeRepairShop.Api.Features.Orders.Models;

namespace BikeRepairShop.Api.Features.Orders.Queries;

public sealed record ListOrdersQuery(string? Search, int Page, int PageSize)
{
    public static ListOrdersQuery From(ListOrdersRequest request)
    {
        var search = request.Search?.Trim();
        return new ListOrdersQuery(string.IsNullOrEmpty(search) ? null : search, request.Page, request.PageSize);
    }
}
