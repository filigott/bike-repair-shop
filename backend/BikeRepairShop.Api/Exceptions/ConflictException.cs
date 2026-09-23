namespace BikeRepairShop.Api.Exceptions;

public sealed class ConflictException(string message) : Exception(message)
{
    public const string OrderChangedMessage = "The order has changed since it was loaded.";

    public static ConflictException OrderChanged()
    {
        return new ConflictException(OrderChangedMessage);
    }
}
