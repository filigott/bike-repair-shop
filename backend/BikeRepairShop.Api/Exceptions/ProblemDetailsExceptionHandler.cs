using BikeRepairShop.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Api.Exceptions;

public sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // A write racing between load and save trips EF's xmin check rather than the handler's version check.
        if (exception is DbUpdateConcurrencyException)
        {
            exception = ConflictException.OrderChanged();
        }

        int? status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException or InvalidStateTransitionException => StatusCodes.Status409Conflict,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            _ => null,
        };

        if (status is null)
        {
            return false;
        }

        var problem = new ProblemDetails { Status = status, Detail = exception.Message };
        problem.Extensions["code"] = CodeOf(exception);

        httpContext.Response.StatusCode = status.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    private static string? CodeOf(Exception exception)
    {
        return exception switch
        {
            DomainException domain => domain.Code.ToString(),
            ApiException api => api.Code.ToString(),
            _ => null,
        };
    }
}
