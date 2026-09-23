namespace BikeRepairShop.Api.Exceptions;

public abstract class ApiException(ApiErrorCode code, string message) : Exception(message)
{
    public ApiErrorCode Code { get; } = code;
}
