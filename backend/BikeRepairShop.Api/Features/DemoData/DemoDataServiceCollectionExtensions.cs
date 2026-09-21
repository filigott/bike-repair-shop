using BikeRepairShop.Api.Features.DemoData.Handlers;

namespace BikeRepairShop.Api.Features.DemoData;

public static class DemoDataServiceCollectionExtensions
{
    public static IServiceCollection AddDemoData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DemoDataOptions>(configuration.GetSection(DemoDataOptions.SectionName));
        services.AddScoped<GetDemoDataStatusHandler>();
        services.AddScoped<ResetDemoDataHandler>();
        services.AddScoped<SeedDemoDataIfEmptyHandler>();
        return services;
    }
}
