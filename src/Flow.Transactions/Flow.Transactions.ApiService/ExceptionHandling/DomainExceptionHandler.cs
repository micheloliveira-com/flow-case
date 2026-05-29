using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Transactions.ApiService.ExceptionHandling;

public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!IsDomainValidationException(exception))
            return false;

        LogDomainValidationFailure(
            httpContext,
            exception);

        SetBadRequestStatusCode(httpContext);

        await WriteProblemDetailsAsync(
            httpContext,
            exception);

        return true;
    }

    private static bool IsDomainValidationException(Exception exception)
    {
        return exception is InvalidOperationException;
    }

    private void LogDomainValidationFailure(
        HttpContext httpContext,
        Exception exception)
    {
        logger.LogWarning(
            exception,
            "Domain validation failed while processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);
    }

    private static void SetBadRequestStatusCode(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }

    private async Task WriteProblemDetailsAsync(
        HttpContext httpContext,
        Exception exception)
    {
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = CreateProblemDetails(exception)
        });
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return new ProblemDetails
        {
            Title = "Domain validation failed",
            Detail = exception.Message,
            Status = StatusCodes.Status400BadRequest
        };
    }
}
