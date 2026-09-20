using System.Text.Json;
using FluentValidation;
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
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || services.GetService(typeof(IValidator<>).MakeGenericType(argument.GetType())) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var failure in result.Errors)
            {
                failures.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName), failure.ErrorMessage);
            }
        }

        if (!failures.IsValid)
        {
            var problem = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, failures);
            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }
}
