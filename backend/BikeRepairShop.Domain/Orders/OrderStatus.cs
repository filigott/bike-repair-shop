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
    private static readonly FrozenDictionary<(OrderStatus From, OrderAction Action), OrderStatus> Edges =
        new Dictionary<(OrderStatus From, OrderAction Action), OrderStatus>
        {
            [(OrderStatus.Received, OrderAction.Start)] = OrderStatus.InProgress,
            [(OrderStatus.Received, OrderAction.Complete)] = OrderStatus.Completed,
            [(OrderStatus.Received, OrderAction.Cancel)] = OrderStatus.Cancelled,
            [(OrderStatus.InProgress, OrderAction.Complete)] = OrderStatus.Completed,
            [(OrderStatus.InProgress, OrderAction.Cancel)] = OrderStatus.Cancelled,
            [(OrderStatus.Completed, OrderAction.Reopen)] = OrderStatus.InProgress,
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<OrderStatus, OrderAction[]> ActionsByStatus =
        Enum.GetValues<OrderStatus>().ToFrozenDictionary(
            status => status,
            status => Edges.Keys.Where(edge => edge.From == status).Select(edge => edge.Action).Order().ToArray());

    // Declared after ActionsByStatus because static initialisers run in declaration order.
    private static readonly FrozenDictionary<OrderStatus, OrderStatus[]> TargetsByStatus =
        Enum.GetValues<OrderStatus>().ToFrozenDictionary(
            status => status,
            status => ActionsByStatus[status].Select(action => Edges[(status, action)]).Distinct().ToArray());

    extension(OrderStatus status)
    {
        public IReadOnlyList<OrderAction> AllowedActions => ActionsByStatus[status];

        public IReadOnlyList<OrderStatus> AllowedTransitions => TargetsByStatus[status];

        public bool IsOpen => status is OrderStatus.Received or OrderStatus.InProgress;

        public bool IsEditable => status.IsOpen;

        public bool Allows(OrderAction action)
        {
            return Edges.ContainsKey((status, action));
        }

        public OrderStatus After(OrderAction action)
        {
            return Edges.TryGetValue((status, action), out var next)
                ? next
                : throw new ArgumentException($"A {status} order has no {action} action.", nameof(action));
        }

        public bool CanTransitionTo(OrderStatus next)
        {
            return TargetsByStatus[status].Contains(next);
        }
    }
}
