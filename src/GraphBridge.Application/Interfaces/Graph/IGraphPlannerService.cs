using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Planner API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphPlannerService
{
    Task<TaskBoardDto> CreateTaskBoardAsync(CreateTaskBoardRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PlannerTaskDto>> GetPendingTasksAsync(int limit = 50, CancellationToken ct = default);
    Task<TaskCompletionSummaryDto> GetTaskSummaryAsync(int days = 7, CancellationToken ct = default);
}
