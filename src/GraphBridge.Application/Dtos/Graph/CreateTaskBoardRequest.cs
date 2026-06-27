namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for creating a Planner task board via Microsoft Graph.
/// </summary>
public class CreateTaskBoardRequest
{
    public string PlanId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> BucketNames { get; set; } = new();
}
