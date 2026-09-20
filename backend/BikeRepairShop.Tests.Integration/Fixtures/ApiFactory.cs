using BikeRepairShop.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public sealed class ApiFactory(string connectionString, bool runMigrations = true) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:BikeRepairShop", connectionString);
        builder.UseSetting(MigrationStartupExtensions.SettingName, runMigrations ? "true" : "false");
    }
}
