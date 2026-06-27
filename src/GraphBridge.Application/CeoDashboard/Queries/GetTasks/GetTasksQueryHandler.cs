using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetTasks;

/// <summary>
/// Handles retrieving pending tasks, capped at 50 tasks.
/// </summary>
public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<PlannerTaskDto>>
{
    private readonly IGraphPlannerService _plannerService;

    public GetTasksQueryHandler(IGraphPlannerService plannerService)
    {
        _plannerService = plannerService;
    }

    public async Task<IReadOnlyList<PlannerTaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _plannerService.GetPendingTasksAsync(50, cancellationToken);
        return tasks;
    }
}
