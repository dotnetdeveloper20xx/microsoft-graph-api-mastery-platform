using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetSecuritySignals;

/// <summary>
/// Handles retrieving security alerts from the past 24 hours, capped at 50 signals.
/// </summary>
public class GetSecuritySignalsQueryHandler : IRequestHandler<GetSecuritySignalsQuery, IReadOnlyList<SecuritySignalDto>>
{
    private readonly IGraphSecurityService _securityService;

    public GetSecuritySignalsQueryHandler(IGraphSecurityService securityService)
    {
        _securityService = securityService;
    }

    public async Task<IReadOnlyList<SecuritySignalDto>> Handle(GetSecuritySignalsQuery request, CancellationToken cancellationToken)
    {
        var alerts = await _securityService.GetRecentAlertsAsync(24, 50, cancellationToken);
        return alerts;
    }
}
