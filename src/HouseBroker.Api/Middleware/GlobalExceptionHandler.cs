using FluentValidation;
using HouseBroker.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HouseBroker.Api.Middleware;

/// <summary>
/// Turns exceptions into RFC 7807 ProblemDetails. Bodies are built by MVC's
/// <see cref="ProblemDetailsFactory"/> (same shape and traceId as automatic 400s) and written
/// by <see cref="IProblemDetailsService"/>, so controllers never build error bodies.
/// </summary>
public sealed class GlobalExceptionHandler(
    ProblemDetailsFactory problemDetailsFactory,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled exception");

        // Never leak internal exception messages on a 500.
        var detail = status == StatusCodes.Status500InternalServerError ? null : ex.Message;

        ProblemDetails problem = ex is ValidationException { Errors: var errors } && errors.Any()
            ? problemDetailsFactory.CreateValidationProblemDetails(ctx, ToModelState(errors), status, title)
            : problemDetailsFactory.CreateProblemDetails(ctx, status, title, detail: detail);

        ctx.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            ProblemDetails = problem,
            Exception = ex
        });
    }

    private static ModelStateDictionary ToModelState(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in errors)
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        return modelState;
    }
}
