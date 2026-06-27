using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetSecuritySignals;

/// <summary>
/// Query to retrieve security signals/alerts from the past 24 hours
/// for the CEO dashboard, limited to a maximum of 50 signals.
/// </summary>
public class GetSecuritySignalsQuery : IRequest<IReadOnlyList<SecuritySignalDto>>
{
}
