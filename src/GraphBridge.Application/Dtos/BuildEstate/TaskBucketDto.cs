namespace GraphBridge.Application.Dtos.BuildEstate;

/// <summary>
/// Represents a bucket within a project task board.
/// </summary>
public class TaskBucketDto
{
    public string Name { get; set; } = string.Empty;
    public List<ProjectTaskDto> Tasks { get; set; } = new();
}
