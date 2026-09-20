namespace BikeRepairShop.Tests.Integration.Fixtures;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Integration";
}
