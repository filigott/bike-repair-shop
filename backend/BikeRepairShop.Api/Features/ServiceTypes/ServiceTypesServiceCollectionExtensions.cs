using BikeRepairShop.Api.Features.ServiceTypes.Handlers;

namespace BikeRepairShop.Api.Features.ServiceTypes;

public static class ServiceTypesServiceCollectionExtensions
{
    public static IServiceCollection AddServiceTypes(this IServiceCollection services)
    {
        services.AddScoped<ListServiceTypesHandler>();
        return services;
    }
}
