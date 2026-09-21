using BikeRepairShop.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BikeRepairShop.Api.Exceptions;

public sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int? status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            InvalidStateTransitionException => StatusCodes.Status409Conflict,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            _ => null,
        };

        if (status is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = status.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Detail = exception.Message },
        });
    }
}
