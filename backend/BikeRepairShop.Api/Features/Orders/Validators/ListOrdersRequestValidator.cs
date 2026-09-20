using BikeRepairShop.Api.Features.Orders.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Features.Orders.Validators;

public sealed class ListOrdersRequestValidator : AbstractValidator<ListOrdersRequest>
{
    public ListOrdersRequestValidator()
    {
        RuleFor(r => r.Search)
            .MaximumLength(ListOrdersRequest.SearchMaxLength);

        RuleFor(r => r.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, ListOrdersRequest.MaxPageSize);
    }
}
