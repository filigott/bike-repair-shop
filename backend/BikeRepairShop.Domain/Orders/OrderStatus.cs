using System.Collections.Frozen;

namespace BikeRepairShop.Domain.Orders;

public enum OrderStatus
{
    Received,
    InProgress,
    Completed,
    Cancelled,
}

public static class OrderStatusExtensions
{
    private static readonly FrozenDictionary<OrderStatus, OrderStatus[]> Transitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Received] = [OrderStatus.InProgress, OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.InProgress] = [OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.Completed] = [OrderStatus.InProgress],
            [OrderStatus.Cancelled] = [],
        }.ToFrozenDictionary();

    extension(OrderStatus status)
    {
        public IReadOnlyList<OrderStatus> AllowedTransitions => Transitions[status];

        public bool IsOpen => status is OrderStatus.Received or OrderStatus.InProgress;

        public bool IsEditable => status.IsOpen;

        public bool CanTransitionTo(OrderStatus next)
        {
            return Transitions[status].Contains(next);
        }
    }
}
