using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Domain.Orders;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.CustomerName)
            .NotEmpty()
            .MaximumLength(MaintenanceOrder.CustomerNameMaxLength);

        RuleFor(r => r.PhoneNumber)
            .NotEmpty()
            .Must(v => PhoneNumber.TryParse(v, out _))
            .WithMessage($"'{{PropertyName}}' must be {PhoneNumber.MinDigits} to {PhoneNumber.MaxDigits} digits with an optional leading '+'.");

        RuleFor(r => r.Email)
            .NotEmpty()
            .Must(v => EmailAddress.TryParse(v, out _))
            .WithMessage("'{PropertyName}' must be a valid email address.");

        RuleFor(r => r.BikeBrand)
            .NotEmpty()
            .MaximumLength(MaintenanceOrder.BikeBrandMaxLength);

        RuleFor(r => r.ServiceType)
            .IsInEnum();

        RuleFor(r => r.DueDate)
            .NotEmpty();

        RuleFor(r => r.Notes)
            .MaximumLength(MaintenanceOrder.NotesMaxLength);
    }
}
