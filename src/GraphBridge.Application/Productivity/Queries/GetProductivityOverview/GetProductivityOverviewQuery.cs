using GraphBridge.Application.Dtos.Productivity;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityOverview;

/// <summary>
/// Query to retrieve the productivity assistant overview with aggregated info.
/// </summary>
public class GetProductivityOverviewQuery : IRequest<ProductivitySummaryDto>
{
}
