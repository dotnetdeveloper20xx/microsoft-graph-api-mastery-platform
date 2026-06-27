namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Task completion summary from Microsoft Graph Planner.
/// </summary>
public class TaskCompletionSummaryDto
{
    public int Completed { get; set; }
    public int Overdue { get; set; }
    public int InProgress { get; set; }
}
