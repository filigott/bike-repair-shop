using BikeRepairShop.Database;
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

    public async Task ResetAsync()
    {
        await using var connection = await OpenConnectionAsync();
        await respawner.ResetAsync(connection);
    }

    public AsyncServiceScope CreateScope()
    {
        return Factory.Services.CreateAsyncScope();
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
