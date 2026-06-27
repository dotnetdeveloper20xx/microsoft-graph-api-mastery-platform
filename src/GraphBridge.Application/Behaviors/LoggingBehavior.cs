using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphBridge.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request entry, exit, and elapsed time
/// with the correlationId for traceability.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.Id ?? Trace.CorrelationManager.ActivityId.ToString();

        _logger.LogInformation(
            "Handling {RequestName} with CorrelationId {CorrelationId}",
            requestName,
            correlationId);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms with CorrelationId {CorrelationId}",
            requestName,
            stopwatch.ElapsedMilliseconds,
            correlationId);

        return response;
    }
}
