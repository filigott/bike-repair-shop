using BikeRepairShop.Api.Features.Orders.Handlers;

namespace BikeRepairShop.Api.Features.Orders;

public static class OrdersServiceCollectionExtensions
{
    public static IServiceCollection AddOrders(this IServiceCollection services)
    {
        services.AddScoped<PlaceOrderHandler>();
        services.AddScoped<ListOrdersHandler>();
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<CorrectOrderDetailsHandler>();
        services.AddScoped<ChangeOrderServiceHandler>();
        services.AddScoped<RescheduleOrderHandler>();
        return services;
    }
}
