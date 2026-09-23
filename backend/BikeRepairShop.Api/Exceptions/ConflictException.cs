namespace BikeRepairShop.Api.Exceptions;

public sealed class ConflictException(ApiErrorCode code, string message) : ApiException(code, message)
{
    public static ConflictException OrderChanged()
    {
        return new ConflictException(ApiErrorCode.OrderChanged, "The order has changed since it was loaded.");
    }
}
