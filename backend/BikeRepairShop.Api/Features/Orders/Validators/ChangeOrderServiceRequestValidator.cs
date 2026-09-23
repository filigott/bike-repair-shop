using BikeRepairShop.Api.Configuration;
using BikeRepairShop.Api.Features.Orders.Models;
using BikeRepairShop.Api.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class ChangeOrderServiceRequestValidator : AbstractValidator<ChangeOrderServiceRequest>
{
    public ChangeOrderServiceRequestValidator()
    {
        RuleFor(r => r.ServiceType).IsInEnum().WithErrorCode(ValidationErrorCode.UnknownValue);
        RuleFor(r => r.Version).NotNull().WithErrorCode(ValidationErrorCode.Required);
    }
}
