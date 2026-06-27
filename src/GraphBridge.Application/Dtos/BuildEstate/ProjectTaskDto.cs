namespace GraphBridge.Application.Dtos.BuildEstate;

/// <summary>
/// Represents a task within a project task board bucket.
/// </summary>
public class ProjectTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}
