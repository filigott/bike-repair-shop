namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record ListOrdersRequest(string? Search = null, int Page = 1, int PageSize = ListOrdersRequest.DefaultPageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 100;
}
