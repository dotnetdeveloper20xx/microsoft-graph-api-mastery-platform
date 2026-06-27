namespace GraphBridge.Application.Dtos.BuildEstate;

/// <summary>
/// Request model for creating a new BuildEstate project.
/// </summary>
public class CreateBuildEstateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PlanningStatus { get; set; } = string.Empty;
    public List<string> Directors { get; set; } = new();
}
