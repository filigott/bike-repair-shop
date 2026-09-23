using BikeRepairShop.Api.Configuration;
using BikeRepairShop.Api.Models;
using BikeRepairShop.Domain.Orders;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public static class OrderFieldRules
{
    public static IRuleBuilderOptions<T, string> ValidCustomerName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithErrorCode(ValidationErrorCode.Required)
            .MaximumLength(MaintenanceOrder.CustomerNameMaxLength).WithErrorCode(ValidationErrorCode.TooLong);
    }

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithErrorCode(ValidationErrorCode.Required)
            .Must(v => PhoneNumber.TryParse(v, out _))
            .WithMessage($"'{{PropertyName}}' must be {PhoneNumber.MinDigits} to {PhoneNumber.MaxDigits} digits with an optional leading '+'.")
            .WithErrorCode(ValidationErrorCode.InvalidPhoneNumber);
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithErrorCode(ValidationErrorCode.Required)
            .Must(v => EmailAddress.TryParse(v, out _))
            .WithMessage("'{PropertyName}' must be a valid email address.")
            .WithErrorCode(ValidationErrorCode.InvalidEmail);
    }

    public static IRuleBuilderOptions<T, string> ValidBikeBrand<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithErrorCode(ValidationErrorCode.Required)
            .MaximumLength(MaintenanceOrder.BikeBrandMaxLength).WithErrorCode(ValidationErrorCode.TooLong);
    }

    public static IRuleBuilderOptions<T, string?> ValidNotes<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule.MaximumLength(MaintenanceOrder.NotesMaxLength).WithErrorCode(ValidationErrorCode.TooLong);
    }
}
