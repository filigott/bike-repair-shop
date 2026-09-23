using BikeRepairShop.Api.Models;
using FluentValidation;

namespace BikeRepairShop.Api.Configuration;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithErrorCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        ValidationErrorCode code)
    {
        return rule.WithErrorCode(code.ToString());
    }
}
