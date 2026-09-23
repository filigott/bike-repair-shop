namespace BikeRepairShop.Api.Exceptions;

public sealed class NotFoundException(ApiErrorCode code, string message) : ApiException(code, message)
{
    public static NotFoundException Order(Guid id)
    {
        return new NotFoundException(ApiErrorCode.OrderNotFound, $"No order with id '{id}'.");
    }
}
