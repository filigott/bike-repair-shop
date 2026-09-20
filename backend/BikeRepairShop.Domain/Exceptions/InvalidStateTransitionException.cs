using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Domain.Exceptions;

public sealed class InvalidStateTransitionException(OrderStatus from, OrderStatus to)
    : DomainException($"An order cannot move from {from} to {to}.")
{
    public OrderStatus From { get; } = from;

    public OrderStatus To { get; } = to;
}
