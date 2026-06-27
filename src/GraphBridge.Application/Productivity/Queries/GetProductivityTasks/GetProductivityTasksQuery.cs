using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityTasks;

/// <summary>
/// Query to retrieve completed, overdue, and in-progress task counts for the past 7 days.
/// </summary>
public class GetProductivityTasksQuery : IRequest<TaskCompletionSummaryDto>
{
}
