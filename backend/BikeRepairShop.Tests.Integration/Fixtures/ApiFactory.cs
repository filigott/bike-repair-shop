using BikeRepairShop.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(MigrationStartupExtensions.SettingName, "false");
    }
}
