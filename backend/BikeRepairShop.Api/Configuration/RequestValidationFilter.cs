using System.Text.Json;
using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Api.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BikeRepairShop.Api.Configuration;

public sealed class RequestValidationFilter(IServiceProvider services, ProblemDetailsFactory problemDetailsFactory) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Fresh state: the binder's own entries carry PascalCase keys and would swallow the camelCase ones.
        var failures = new ModelStateDictionary();
        var codes = new Dictionary<string, List<string>>();
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || services.GetService(typeof(IValidator<>).MakeGenericType(argument.GetType())) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var failure in result.Errors)
            {
                var field = JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName);
                failures.AddModelError(field, failure.ErrorMessage);
                codes.TryAdd(field, []);
                codes[field].Add(CodeOf(failure).ToString());
            }
        }

        if (!failures.IsValid)
        {
            var problem = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, failures);
            problem.Extensions["code"] = ApiErrorCode.InvalidRequest.ToString();
            problem.Extensions["errorCodes"] = codes;
            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }

    private static ValidationErrorCode CodeOf(ValidationFailure failure)
    {
        return Enum.TryParse<ValidationErrorCode>(failure.ErrorCode, out var code) && Enum.IsDefined(code)
            ? code
            : ValidationErrorCode.Invalid;
    }
}
