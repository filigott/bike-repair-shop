using BikeRepairShop.Database;
using BikeRepairShop.Domain.Orders;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace BikeRepairShop.Tests.Integration.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:18").Build();
    private Respawner respawner = null!;

    public ApiFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync();
        Factory = new ApiFactory(container.GetConnectionString());
        _ = Factory.Services;

        await using var connection = await OpenConnectionAsync();
        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["service_types", "__EFMigrationsHistory"],
        });
    }

    public TestTimeProvider Time => Factory.Time;

    public async Task ResetAsync()
    {
        Time.Reset();
        await using var connection = await OpenConnectionAsync();
        await respawner.ResetAsync(connection);
    }

    public AsyncServiceScope CreateScope()
    {
        return Factory.Services.CreateAsyncScope();
    }

    public async Task SeedAsync(params MaintenanceOrder[] orders)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BikeRepairShopDbContext>();
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await container.DisposeAsync();
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(container.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}
