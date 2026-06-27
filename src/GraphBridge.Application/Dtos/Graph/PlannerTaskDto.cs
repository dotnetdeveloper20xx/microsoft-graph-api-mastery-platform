namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a Planner task from Microsoft Graph.
/// </summary>
public class PlannerTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}
