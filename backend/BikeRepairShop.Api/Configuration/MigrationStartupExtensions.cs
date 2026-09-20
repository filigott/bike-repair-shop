using BikeRepairShop.Database;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Configuration;

public static class MigrationStartupExtensions
{
    public const string SettingName = "RunMigrationsOnStartup";

    public static async Task RunMigrationsIfEnabledAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>(SettingName))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>().Database.MigrateAsync();
    }
}
