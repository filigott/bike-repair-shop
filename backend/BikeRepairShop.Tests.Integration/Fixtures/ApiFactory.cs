using BikeRepairShop.Api.Configuration;
using BikeRepairShop.Api.Features.DemoData;
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
        // The Development settings file switches demo data on; a test host decides for itself.
        builder.UseSetting($"{DemoDataOptions.SectionName}:Enabled", "false");
        builder.ConfigureTestServices(services => services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Time)));
    }
}
