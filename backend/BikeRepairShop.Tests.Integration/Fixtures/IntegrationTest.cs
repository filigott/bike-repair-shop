namespace BikeRepairShop.Tests.Integration.Fixtures;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTest(DatabaseFixture database) : IAsyncLifetime
{
    protected DatabaseFixture Database { get; } = database;

    public async ValueTask InitializeAsync()
    {
        await Database.ResetAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
