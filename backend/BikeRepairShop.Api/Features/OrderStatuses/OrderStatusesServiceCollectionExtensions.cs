using BikeRepairShop.Api.Features.OrderStatuses.Handlers;

namespace BikeRepairShop.Api.Features.OrderStatuses;

public static class OrderStatusesServiceCollectionExtensions
{
    public static IServiceCollection AddOrderStatuses(this IServiceCollection services)
    {
        services.AddScoped<ListOrderStatusesHandler>();
        return services;
    }
}
