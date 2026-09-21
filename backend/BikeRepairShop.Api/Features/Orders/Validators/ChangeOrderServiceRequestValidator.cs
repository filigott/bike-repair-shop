using BikeRepairShop.Api.Features.Orders.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class ChangeOrderServiceRequestValidator : AbstractValidator<ChangeOrderServiceRequest>
{
    public ChangeOrderServiceRequestValidator()
    {
        RuleFor(r => r.ServiceType).IsInEnum();
    }
}
