using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Api.Features.Orders.Models;

public sealed record ListOrdersRequest(
    [property: FromQuery(Name = "search")] string? Search = null,
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "pageSize")] int PageSize = ListOrdersRequest.DefaultPageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 100;
}
