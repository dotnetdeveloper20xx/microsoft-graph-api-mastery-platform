using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using GraphBridge.Shared;
using GraphBridge.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using AuthenticationException = GraphBridge.Shared.Exceptions.AuthenticationException;

namespace GraphBridge.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions,
/// maps them to appropriate HTTP status codes, and returns a standardised API_Envelope response.
/// Stack traces and internal exception details are never exposed in responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogError(
            exception,
            "An unhandled exception occurred. CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}",
            correlationId,
            exception.GetType().Name);

        var (statusCode, message, errors) = MapException(exception);

        var envelope = ApiEnvelope<object>.Fail(message, errors);
        envelope.CorrelationId = correlationId;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string Message, List<ApiError> Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationEx.Errors.Select(e => new ApiError
                {
                    Field = e.PropertyName,
                    Detail = e.ErrorMessage
                }).ToList()
            ),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                notFoundEx.Message,
                new List<ApiError>()
            ),

            BusinessRuleException businessRuleEx => (
                StatusCodes.Status422UnprocessableEntity,
                businessRuleEx.Message,
                new List<ApiError>()
            ),

            GraphServiceException => (
                StatusCodes.Status502BadGateway,
                "A downstream service error occurred",
                new List<ApiError>()
            ),

            AuthenticationException authEx => (
                StatusCodes.Status401Unauthorized,
                authEx.Message,
                new List<ApiError>()
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please reference the correlationId for support.",
                new List<ApiError>()
            )
        };
    }
}
