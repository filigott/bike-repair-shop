using BikeRepairShop.Api.Features.DemoData;
using BikeRepairShop.Api.Features.DemoData.Handlers;
using BikeRepairShop.Database;
using BikeRepairShop.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public static class DemoDataFixtures
{
    public static ResetDemoDataHandler SwitchedOnReset(AsyncServiceScope scope, IShopClock clock)
    {
        return new ResetDemoDataHandler(
            scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>(),
            clock,
            Options.Create(new DemoDataOptions { Enabled = true }));
    }
}
