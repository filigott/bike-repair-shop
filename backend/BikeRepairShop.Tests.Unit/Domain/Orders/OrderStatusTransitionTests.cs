using AwesomeAssertions;
using BikeRepairShop.Domain.Orders;

namespace BikeRepairShop.Tests.Unit.Domain.Orders;

public class OrderStatusTransitionTests
{
    [Theory]
    [InlineData(OrderStatus.Received, OrderStatus.InProgress, true)]
    [InlineData(OrderStatus.Received, OrderStatus.Completed, true)]
    [InlineData(OrderStatus.Received, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Received, OrderStatus.Received, false)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Completed, true)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Received, false)]
    [InlineData(OrderStatus.InProgress, OrderStatus.InProgress, false)]
    [InlineData(OrderStatus.Completed, OrderStatus.InProgress, true)]
    [InlineData(OrderStatus.Completed, OrderStatus.Received, false)]
    [InlineData(OrderStatus.Completed, OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Completed, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Received, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.InProgress, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled, false)]
    public void CanTransitionTo_follows_the_matrix(OrderStatus from, OrderStatus to, bool expected)
    {
        // Act
        var result = from.CanTransitionTo(to);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(OrderStatus.Received, new[] { OrderAction.Start, OrderAction.Complete, OrderAction.Cancel })]
    [InlineData(OrderStatus.InProgress, new[] { OrderAction.Complete, OrderAction.Cancel })]
    [InlineData(OrderStatus.Completed, new[] { OrderAction.Reopen })]
    [InlineData(OrderStatus.Cancelled, new OrderAction[0])]
    public void AllowedActions_lists_every_move_the_status_offers(OrderStatus from, OrderAction[] expected)
    {
        // Act
        var result = from.AllowedActions;

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(OrderStatus.Received, OrderAction.Start, true)]
    [InlineData(OrderStatus.Received, OrderAction.Reopen, false)]
    [InlineData(OrderStatus.InProgress, OrderAction.Start, false)]
    [InlineData(OrderStatus.Completed, OrderAction.Start, false)]
    [InlineData(OrderStatus.Completed, OrderAction.Reopen, true)]
    [InlineData(OrderStatus.Cancelled, OrderAction.Reopen, false)]
    public void Allows_separates_start_from_reopen_by_origin(OrderStatus from, OrderAction action, bool expected)
    {
        // Act
        var result = from.Allows(action);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(OrderStatus.Received, new[] { OrderStatus.InProgress, OrderStatus.Completed, OrderStatus.Cancelled })]
    [InlineData(OrderStatus.InProgress, new[] { OrderStatus.Completed, OrderStatus.Cancelled })]
    [InlineData(OrderStatus.Completed, new[] { OrderStatus.InProgress })]
    [InlineData(OrderStatus.Cancelled, new OrderStatus[0])]
    public void AllowedTransitions_lists_every_legal_target(OrderStatus from, OrderStatus[] expected)
    {
        // Act
        var result = from.AllowedTransitions;

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(OrderStatus.Received, true)]
    [InlineData(OrderStatus.InProgress, true)]
    [InlineData(OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Cancelled, false)]
    public void IsEditable_only_while_open(OrderStatus status, bool expected)
    {
        // Act
        var result = status.IsEditable;

        // Assert
        result.Should().Be(expected);
    }
}
