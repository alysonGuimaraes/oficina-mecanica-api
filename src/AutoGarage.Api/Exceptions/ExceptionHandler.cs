using AutoGarage.Communication.Responses.Errors;
using AutoGarage.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace AutoGarage.Api.Exceptions;

public class ExceptionHandler : IExceptionHandler
{
    // TODO: Refactor to not use hardcoded url
    private const string BaseErrorTypeUrl = "https://api.autogarage.com/errors/";

    private readonly IProblemDetailsService _problemDetailsService;

    public ExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ErrorResponse? errorResponse;

        if (exception is DomainException domainException)
        {
            errorResponse = BuildDomainError(httpContext, domainException);
        }
        else if (exception is ErrorOnValidationException validationException)
        {
            errorResponse = BuildOnValidationException(httpContext, validationException);
        }
        else
        {
            errorResponse = BuildUnknownError(httpContext);
        }

        httpContext.Response.StatusCode = errorResponse.Status ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = errorResponse
        });
    }

    private static ErrorResponse BuildDomainError(HttpContext httpContext, DomainException domainException)
    {
        var statusCode = ResolveDomainErrorStatusCode(domainException);

        return new ErrorResponse
        {
            Type = $"{BaseErrorTypeUrl}{domainException.ErrorType}",
            Title = domainException.ErrorType,
            Status = statusCode,
            Detail = domainException.Message,
            Instance = httpContext.Request.Path.Value,
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier
            }
        };
    }

    private static int ResolveDomainErrorStatusCode(DomainException domainException) => domainException switch
    {
        NotFoundDomainException => StatusCodes.Status404NotFound,
        ConflictDomainException => StatusCodes.Status409Conflict,
        BadRequestDomainException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status422UnprocessableEntity
    };

    private static ErrorResponse BuildOnValidationException(HttpContext httpContext, ErrorOnValidationException validationException)
    {
        const int statusCode = StatusCodes.Status400BadRequest;

        var invalidParams = validationException.ErrorsMessages;

        return new ErrorResponse
        {
            Type = $"{BaseErrorTypeUrl}validation-error",
            Title = "Validation Error",
            Status = statusCode,
            Detail = "One or more fields are invalid.",
            Instance = httpContext.Request.Path.Value,
            Extensions =
            {
                ["invalid-params"] = invalidParams
            }
        };
    }

    private static ErrorResponse BuildUnknownError(HttpContext httpContext)
    {
        const int statusCode = StatusCodes.Status500InternalServerError;

        return new ErrorResponse
        {
            Type = $"{BaseErrorTypeUrl}server-error",
            Title = "Internal Server Error",
            Status = statusCode,
            Detail = "An unexpected error occurred while processing your request.",
            Instance = httpContext.Request.Path.Value,
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier
            }
        };
    }
}
