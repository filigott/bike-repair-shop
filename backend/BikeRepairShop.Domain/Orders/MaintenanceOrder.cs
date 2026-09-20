using BikeRepairShop.Domain.Exceptions;

namespace BikeRepairShop.Domain.Orders;

public sealed class MaintenanceOrder
{
    public const int CustomerNameMaxLength = 100;
    public const int BikeBrandMaxLength = 50;
    public const int NotesMaxLength = 1000;

    private MaintenanceOrder()
    {
    }

    public Guid Id { get; init; }

    public string CustomerName { get; private set; } = null!;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public EmailAddress Email { get; private set; } = null!;

    public string BikeBrand { get; private set; } = null!;

    public ServiceType ServiceType { get; private set; }

    public DateOnly DueDate { get; private set; }

    public string? Notes { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset PlacedAt { get; private set; }

    public IReadOnlyList<OrderStatus> AllowedTransitions => Status.AllowedTransitions;

    public bool IsOverdueOn(DateOnly today)
    {
        return Status.IsOpen && DueDate < today;
    }

    public static MaintenanceOrder Place(
        string customerName,
        PhoneNumber phoneNumber,
        EmailAddress email,
        string bikeBrand,
        ServiceType serviceType,
        DateOnly dueDate,
        string? notes,
        IShopClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        var order = new MaintenanceOrder
        {
            Id = Guid.CreateVersion7(),
            Status = OrderStatus.Received,
            PlacedAt = clock.Now,
        };
        order.SetDetails(customerName, phoneNumber, email, bikeBrand, notes);
        order.SetService(serviceType);
        order.SetDueDate(dueDate, clock.Today);
        return order;
    }

    public void CorrectDetails(
        string customerName,
        PhoneNumber phoneNumber,
        EmailAddress email,
        string bikeBrand,
        string? notes)
    {
        EnsureEditable();
        SetDetails(customerName, phoneNumber, email, bikeBrand, notes);
    }

    public void ChangeService(ServiceType serviceType)
    {
        EnsureEditable();
        SetService(serviceType);
    }

    public void Reschedule(DateOnly dueDate, IShopClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        EnsureEditable();
        SetDueDate(dueDate, clock.Today);
    }

    public void Start()
    {
        TransitionTo(OrderStatus.InProgress);
    }

    public void Complete()
    {
        TransitionTo(OrderStatus.Completed);
    }

    public void Cancel()
    {
        TransitionTo(OrderStatus.Cancelled);
    }

    public void Reopen()
    {
        TransitionTo(OrderStatus.InProgress);
    }

    private void TransitionTo(OrderStatus next)
    {
        if (!Status.CanTransitionTo(next))
        {
            throw new InvalidStateTransitionException(Status, next);
        }

        Status = next;
    }

    private void EnsureEditable()
    {
        if (!Status.IsEditable)
        {
            throw new BusinessRuleViolationException($"A {Status} order cannot be changed.");
        }
    }

    private void SetDetails(
        string customerName,
        PhoneNumber phoneNumber,
        EmailAddress email,
        string bikeBrand,
        string? notes)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(email);

        CustomerName = RequiredText(customerName, CustomerNameMaxLength, nameof(customerName));
        BikeBrand = RequiredText(bikeBrand, BikeBrandMaxLength, nameof(bikeBrand));
        Notes = OptionalText(notes, NotesMaxLength, nameof(notes));
        PhoneNumber = phoneNumber;
        Email = email;
    }

    private void SetService(ServiceType serviceType)
    {
        if (!Enum.IsDefined(serviceType))
        {
            throw new ArgumentException($"'{serviceType}' is not a known service type.", nameof(serviceType));
        }

        ServiceType = serviceType;
    }

    private void SetDueDate(DateOnly dueDate, DateOnly today)
    {
        if (dueDate < today)
        {
            throw new BusinessRuleViolationException("The due date cannot be before today.");
        }

        DueDate = dueDate;
    }

    private static string RequiredText(string value, int maxLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return CappedText(value.Trim(), maxLength, paramName);
    }

    private static string? OptionalText(string? value, int maxLength, string paramName)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : CappedText(trimmed, maxLength, paramName);
    }

    private static string CappedText(string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"'{paramName}' cannot exceed {maxLength} characters.", paramName);
        }

        return value;
    }
}
