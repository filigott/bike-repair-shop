using BikeRepairShop.Domain.Orders;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public static class OrderFieldRules
{
    public static IRuleBuilderOptions<T, string> ValidCustomerName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().MaximumLength(MaintenanceOrder.CustomerNameMaxLength);
    }

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .Must(v => PhoneNumber.TryParse(v, out _))
            .WithMessage($"'{{PropertyName}}' must be {PhoneNumber.MinDigits} to {PhoneNumber.MaxDigits} digits with an optional leading '+'.");
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .Must(v => EmailAddress.TryParse(v, out _))
            .WithMessage("'{PropertyName}' must be a valid email address.");
    }

    public static IRuleBuilderOptions<T, string> ValidBikeBrand<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().MaximumLength(MaintenanceOrder.BikeBrandMaxLength);
    }

    public static IRuleBuilderOptions<T, string?> ValidNotes<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule.MaximumLength(MaintenanceOrder.NotesMaxLength);
    }
}
