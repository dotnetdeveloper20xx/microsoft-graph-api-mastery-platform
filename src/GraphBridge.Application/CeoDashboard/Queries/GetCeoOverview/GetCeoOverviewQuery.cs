using GraphBridge.Application.Dtos.CeoDashboard;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetCeoOverview;

/// <summary>
/// Query to retrieve the CEO command centre overview with aggregated counts
/// from calendar, mail, planner, drive, and security services.
/// </summary>
public class GetCeoOverviewQuery : IRequest<CeoDashboardOverviewDto>
{
}
