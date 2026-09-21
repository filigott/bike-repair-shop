using BikeRepairShop.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public sealed class ApiFactory(string connectionString, bool runMigrations = true) : WebApplicationFactory<Program>
{
    public TestTimeProvider Time { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:BikeRepairShop", connectionString);
        builder.UseSetting(MigrationStartupExtensions.SettingName, runMigrations ? "true" : "false");
        builder.ConfigureTestServices(services => services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Time)));
    }
}
