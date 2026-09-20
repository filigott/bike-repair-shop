using BikeRepairShop.Domain;

namespace BikeRepairShop.Api.Configuration;

public static class ShopClockServiceCollectionExtensions
{
    public static IServiceCollection AddShopClock(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ShopOptions>()
            .Bind(configuration.GetSection(ShopOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IShopClock, ShopClock>();
        return services;
    }
}
