namespace GraphBridge.Domain.Entities;

public class BuildEstateProject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PlanningStatus { get; set; } = string.Empty;
    public List<string> Directors { get; set; } = new();
    public bool WorkspaceLaunched { get; set; }
    public bool TaskBoardCreated { get; set; }
}
