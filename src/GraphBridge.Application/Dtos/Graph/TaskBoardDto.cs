namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a Planner task board from Microsoft Graph.
/// </summary>
public class TaskBoardDto
{
    public List<TaskBucketDto> Buckets { get; set; } = new();
}

/// <summary>
/// Represents a bucket within a Planner task board.
/// </summary>
public class TaskBucketDto
{
    public string Name { get; set; } = string.Empty;
    public List<ProjectTaskDto> Tasks { get; set; } = new();
}

/// <summary>
/// Represents a task within a task board bucket.
/// </summary>
public class ProjectTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}
