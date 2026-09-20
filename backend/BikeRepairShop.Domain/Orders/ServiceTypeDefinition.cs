namespace BikeRepairShop.Domain.Orders;

public sealed class ServiceTypeDefinition
{
    public const int DisplayNameMaxLength = 100;

    private ServiceTypeDefinition()
    {
    }

    public ServiceType Code { get; init; }

    public string DisplayName { get; init; } = null!;

    public int SortOrder { get; init; }
}
