using BikeRepairShop.Api.Features.DemoData.Handlers;
using Microsoft.Extensions.Options;

namespace BikeRepairShop.Api.Features.DemoData;

public static class DemoDataStartupExtensions
{
    public static async Task SeedDemoDataIfEnabledAsync(this WebApplication app)
    {
        if (!app.Services.GetRequiredService<IOptions<DemoDataOptions>>().Value.Enabled)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var seeded = await scope.ServiceProvider.GetRequiredService<SeedDemoDataIfEmptyHandler>().HandleAsync(default);

        app.Logger.LogInformation("Demo data is enabled. Orders seeded on startup: {SeededCount}", seeded);
    }
}
