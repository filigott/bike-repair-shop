namespace BikeRepairShop.Domain.Exceptions;

public abstract class DomainException(DomainErrorCode code, string message) : Exception(message)
{
    public DomainErrorCode Code { get; } = code;
}
