using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Domain.Exceptions;

public sealed class InvalidStateTransitionException(OrderStatus from, OrderAction action)
    : DomainException($"The {action} action is not allowed on a {from} order.")
{
    public OrderStatus From { get; } = from;

    public OrderAction Action { get; } = action;
}
