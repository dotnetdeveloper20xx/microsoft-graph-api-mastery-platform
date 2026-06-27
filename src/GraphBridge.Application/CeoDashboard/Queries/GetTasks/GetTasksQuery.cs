using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetTasks;

/// <summary>
/// Query to retrieve pending tasks for the CEO dashboard,
/// limited to a maximum of 50 tasks.
/// </summary>
public class GetTasksQuery : IRequest<IReadOnlyList<PlannerTaskDto>>
{
}
