using BikeRepairShop.Api.Features.Orders.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.CustomerName).ValidCustomerName();
        RuleFor(r => r.PhoneNumber).ValidPhoneNumber();
        RuleFor(r => r.Email).ValidEmail();
        RuleFor(r => r.BikeBrand).ValidBikeBrand();
        RuleFor(r => r.ServiceType).IsInEnum();
        RuleFor(r => r.DueDate).NotNull();
        RuleFor(r => r.Notes).ValidNotes();
    }
}
