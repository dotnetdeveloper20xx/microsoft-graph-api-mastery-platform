using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Abstract base controller providing MediatR accessor and common API route prefix.
/// All API controllers should inherit from this class.
/// </summary>
[ApiController]
[Route("api")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Lazily resolved MediatR mediator instance for dispatching commands and queries.
    /// </summary>
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Retrieves the correlation ID assigned to the current request by the CorrelationIdMiddleware.
    /// </summary>
    protected string GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString() ?? string.Empty;
}
