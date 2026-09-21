namespace BikeRepairShop.Api.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException Order(Guid id)
    {
        return new NotFoundException($"No order with id '{id}'.");
    }
}
