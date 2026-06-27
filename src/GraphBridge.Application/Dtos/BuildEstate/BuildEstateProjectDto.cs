namespace GraphBridge.Application.Dtos.BuildEstate;

/// <summary>
/// Represents a BuildEstate project record.
/// </summary>
public class BuildEstateProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PlanningStatus { get; set; } = string.Empty;
    public List<string> Directors { get; set; } = new();
    public bool WorkspaceLaunched { get; set; }
    public bool TaskBoardCreated { get; set; }
}
