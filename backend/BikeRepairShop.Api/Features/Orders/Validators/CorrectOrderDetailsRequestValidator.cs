using BikeRepairShop.Api.Features.Orders.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class CorrectOrderDetailsRequestValidator : AbstractValidator<CorrectOrderDetailsRequest>
{
    public CorrectOrderDetailsRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.CustomerName).ValidCustomerName();
        RuleFor(r => r.PhoneNumber).ValidPhoneNumber();
        RuleFor(r => r.Email).ValidEmail();
        RuleFor(r => r.BikeBrand).ValidBikeBrand();
        RuleFor(r => r.Notes).ValidNotes();
    }
}
