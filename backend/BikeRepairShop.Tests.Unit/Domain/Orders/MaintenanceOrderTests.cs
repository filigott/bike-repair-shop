using AwesomeAssertions;
using BikeRepairShop.Domain.Exceptions;
using BikeRepairShop.Domain.Orders;
using BikeRepairShop.Tests.Unit.Fixtures;

namespace BikeRepairShop.Tests.Unit.Domain.Orders;

public class MaintenanceOrderTests
{
    #region Place

    [Fact]
    public void Place_starts_as_received_with_the_given_details()
    {
        // Arrange
        var dueDate = OrderFixtures.Today.AddDays(5);

        // Act
        var order = OrderFixtures.Placed(dueDate: dueDate);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.Status.Should().Be(OrderStatus.Received);
        order.CustomerName.Should().Be("Kari Nordmann");
        order.PhoneNumber.Value.Should().Be("+4791234567");
        order.Email.Value.Should().Be("kari@example.com");
        order.BikeBrand.Should().Be("Trek");
        order.ServiceType.Should().Be(ServiceType.BrakeMaintenance);
        order.DueDate.Should().Be(dueDate);
        order.Notes.Should().Be("Squeaks when braking");
        order.PlacedAt.Should().Be(OrderFixtures.Clock.Now);
    }

    [Fact]
    public void Place_accepts_a_due_date_of_today()
    {
        // Act
        var order = OrderFixtures.Placed(dueDate: OrderFixtures.Today);

        // Assert
        order.DueDate.Should().Be(OrderFixtures.Today);
    }

    [Fact]
    public void Place_rejects_a_due_date_in_the_past()
    {
        // Arrange
        var yesterday = OrderFixtures.Today.AddDays(-1);

        // Act
        var act = () => OrderFixtures.Placed(dueDate: yesterday);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Place_trims_free_text_and_drops_blank_notes()
    {
        // Act
        var order = OrderFixtures.Placed(customerName: "  Ola  ", bikeBrand: " Giant ", notes: "   ");

        // Assert
        order.CustomerName.Should().Be("Ola");
        order.BikeBrand.Should().Be("Giant");
        order.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Place_rejects_a_blank_customer_name(string customerName)
    {
        // Act
        var act = () => OrderFixtures.Placed(customerName: customerName);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("customerName");
    }

    [Fact]
    public void Place_rejects_a_customer_name_over_the_cap()
    {
        // Arrange
        var customerName = new string('x', MaintenanceOrder.CustomerNameMaxLength + 1);

        // Act
        var act = () => OrderFixtures.Placed(customerName: customerName);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("customerName");
    }

    [Fact]
    public void Place_rejects_a_blank_bike_brand()
    {
        // Act
        var act = () => OrderFixtures.Placed(bikeBrand: " ");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("bikeBrand");
    }

    [Fact]
    public void Place_rejects_a_bike_brand_over_the_cap()
    {
        // Arrange
        var bikeBrand = new string('x', MaintenanceOrder.BikeBrandMaxLength + 1);

        // Act
        var act = () => OrderFixtures.Placed(bikeBrand: bikeBrand);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("bikeBrand");
    }

    [Fact]
    public void Place_rejects_notes_over_the_cap()
    {
        // Arrange
        var notes = new string('x', MaintenanceOrder.NotesMaxLength + 1);

        // Act
        var act = () => OrderFixtures.Placed(notes: notes);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("notes");
    }

    [Fact]
    public void Place_rejects_an_undefined_service_type()
    {
        // Act
        var act = () => OrderFixtures.Placed(serviceType: (ServiceType)99);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("serviceType");
    }

    #endregion

    #region Lifecycle

    [Fact]
    public void Start_moves_a_received_order_to_in_progress()
    {
        // Arrange
        var order = OrderFixtures.Placed();

        // Act
        order.Start();

        // Assert
        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void Complete_moves_an_in_progress_order_to_completed()
    {
        // Arrange
        var order = OrderFixtures.InProgress();

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Complete_moves_a_received_order_straight_to_completed()
    {
        // Arrange
        var order = OrderFixtures.Placed();

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Cancel_moves_an_in_progress_order_to_cancelled()
    {
        // Arrange
        var order = OrderFixtures.InProgress();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Reopen_moves_a_completed_order_back_to_in_progress()
    {
        // Arrange
        var order = OrderFixtures.Completed();

        // Act
        order.Reopen();

        // Assert
        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void Start_throws_when_the_order_is_already_in_progress()
    {
        // Arrange
        var order = OrderFixtures.InProgress();

        // Act
        var act = () => order.Start();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>()
            .Which.Should().BeEquivalentTo(new { From = OrderStatus.InProgress, Action = OrderAction.Start });
        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void Start_throws_when_the_order_is_completed()
    {
        // Arrange
        var order = OrderFixtures.Completed();

        // Act
        var act = () => order.Start();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Reopen_throws_when_the_order_is_received()
    {
        // Arrange
        var order = OrderFixtures.Placed();

        // Act
        var act = () => order.Reopen();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
        order.Status.Should().Be(OrderStatus.Received);
    }

    [Fact]
    public void Cancel_throws_when_the_order_is_completed()
    {
        // Arrange
        var order = OrderFixtures.Completed();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Reopen_throws_when_the_order_is_cancelled()
    {
        // Arrange
        var order = OrderFixtures.Cancelled();

        // Act
        var act = () => order.Reopen();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void AllowedTransitions_reflects_the_current_status()
    {
        // Arrange
        var order = OrderFixtures.Completed();

        // Act
        var result = order.AllowedTransitions;

        // Assert
        result.Should().BeEquivalentTo([OrderStatus.InProgress]);
    }

    [Theory]
    [InlineData(OrderStatus.Received, true)]
    [InlineData(OrderStatus.InProgress, true)]
    [InlineData(OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Cancelled, false)]
    public void IsOverdueOn_is_true_only_for_an_open_order_past_its_due_date(OrderStatus status, bool expected)
    {
        // Arrange
        var order = OrderFixtures.InStatus(status);

        // Act
        var overdue = order.IsOverdueOn(order.DueDate.AddDays(1));

        // Assert
        overdue.Should().Be(expected);
    }

    [Fact]
    public void IsOverdueOn_is_false_on_the_due_date_itself()
    {
        // Arrange
        var order = OrderFixtures.Placed();

        // Act & Assert
        order.IsOverdueOn(order.DueDate).Should().BeFalse();
    }

    #endregion

    #region Editing

    [Fact]
    public void CorrectDetails_replaces_the_contact_and_bike_details()
    {
        // Arrange
        var order = OrderFixtures.InProgress();

        // Act
        order.CorrectDetails(
            "Ola Nordmann",
            PhoneNumber.Parse("98765432"),
            EmailAddress.Parse("Ola@Example.com"),
            "Giant",
            null);

        // Assert
        order.CustomerName.Should().Be("Ola Nordmann");
        order.PhoneNumber.Value.Should().Be("98765432");
        order.Email.Value.Should().Be("ola@example.com");
        order.BikeBrand.Should().Be("Giant");
        order.Notes.Should().BeNull();
    }

    [Fact]
    public void CorrectDetails_throws_when_the_order_is_completed()
    {
        // Arrange
        var order = OrderFixtures.Completed();

        // Act
        var act = () => order.CorrectDetails(
            "Ola Nordmann",
            PhoneNumber.Parse("98765432"),
            EmailAddress.Parse("ola@example.com"),
            "Giant",
            null);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
        order.CustomerName.Should().Be("Kari Nordmann");
    }

    [Fact]
    public void CorrectDetails_is_allowed_again_after_reopening()
    {
        // Arrange
        var order = OrderFixtures.Completed();
        order.Reopen();

        // Act
        order.CorrectDetails(
            "Ola Nordmann",
            PhoneNumber.Parse("98765432"),
            EmailAddress.Parse("ola@example.com"),
            "Giant",
            null);

        // Assert
        order.CustomerName.Should().Be("Ola Nordmann");
    }

    [Fact]
    public void ChangeService_replaces_the_service_type()
    {
        // Arrange
        var order = OrderFixtures.Placed();

        // Act
        order.ChangeService(ServiceType.ChainReplacement);

        // Assert
        order.ServiceType.Should().Be(ServiceType.ChainReplacement);
    }

    [Fact]
    public void ChangeService_throws_when_the_order_is_cancelled()
    {
        // Arrange
        var order = OrderFixtures.Cancelled();

        // Act
        var act = () => order.ChangeService(ServiceType.ChainReplacement);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
        order.ServiceType.Should().Be(ServiceType.BrakeMaintenance);
    }

    [Fact]
    public void Reschedule_moves_the_due_date()
    {
        // Arrange
        var order = OrderFixtures.Placed();
        var newDueDate = OrderFixtures.Today.AddDays(10);

        // Act
        order.Reschedule(newDueDate, OrderFixtures.Clock);

        // Assert
        order.DueDate.Should().Be(newDueDate);
    }

    [Fact]
    public void Reschedule_rejects_a_due_date_in_the_past()
    {
        // Arrange
        var order = OrderFixtures.Placed();
        var originalDueDate = order.DueDate;

        // Act
        var act = () => order.Reschedule(OrderFixtures.Today.AddDays(-1), OrderFixtures.Clock);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
        order.DueDate.Should().Be(originalDueDate);
    }

    [Fact]
    public void Reschedule_throws_when_the_order_is_completed()
    {
        // Arrange
        var order = OrderFixtures.Completed();

        // Act
        var act = () => order.Reschedule(OrderFixtures.Today.AddDays(10), OrderFixtures.Clock);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
    }

    #endregion
}
