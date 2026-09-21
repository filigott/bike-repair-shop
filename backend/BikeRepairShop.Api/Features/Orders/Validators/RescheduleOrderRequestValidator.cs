using BikeRepairShop.Api.Features.Orders.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class RescheduleOrderRequestValidator : AbstractValidator<RescheduleOrderRequest>
{
    public RescheduleOrderRequestValidator()
    {
        RuleFor(r => r.DueDate).NotNull();
    }
}
