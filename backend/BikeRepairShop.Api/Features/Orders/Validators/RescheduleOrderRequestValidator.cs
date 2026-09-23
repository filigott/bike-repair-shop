using BikeRepairShop.Api.Configuration;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Api.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class RescheduleOrderRequestValidator : AbstractValidator<RescheduleOrderRequest>
{
    public RescheduleOrderRequestValidator()
    {
        RuleFor(r => r.DueDate).NotNull().WithErrorCode(ValidationErrorCode.Required);
        RuleFor(r => r.Version).NotNull().WithErrorCode(ValidationErrorCode.Required);
    }
}
