namespace BikeRepairShop.Domain.Exceptions;

public sealed class BusinessRuleViolationException(DomainErrorCode code, string message) : DomainException(code, message);
