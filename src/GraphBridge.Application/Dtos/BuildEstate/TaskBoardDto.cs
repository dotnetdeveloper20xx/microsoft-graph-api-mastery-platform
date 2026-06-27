namespace GraphBridge.Application.Dtos.BuildEstate;

/// <summary>
/// Represents a project task board with buckets and tasks.
/// </summary>
public class TaskBoardDto
{
    public List<TaskBucketDto> Buckets { get; set; } = new();
}
