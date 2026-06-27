using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityTasks;

/// <summary>
/// Handles retrieving task completion counts (completed, overdue, in-progress) for the past 7 days.
/// </summary>
public class GetProductivityTasksQueryHandler : IRequestHandler<GetProductivityTasksQuery, TaskCompletionSummaryDto>
{
    private readonly IGraphPlannerService _plannerService;

    public GetProductivityTasksQueryHandler(IGraphPlannerService plannerService)
    {
        _plannerService = plannerService;
    }

    public async Task<TaskCompletionSummaryDto> Handle(GetProductivityTasksQuery request, CancellationToken cancellationToken)
    {
        return await _plannerService.GetTaskSummaryAsync(7, cancellationToken);
    }
}
