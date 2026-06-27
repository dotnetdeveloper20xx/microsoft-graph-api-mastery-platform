namespace GraphBridge.Application.Dtos.BuildEstate;

/// <summary>
/// Represents a weekly project report summary.
/// </summary>
public class WeeklyReportDto
{
    public int TasksToDo { get; set; }
    public int TasksInProgress { get; set; }
    public int TasksCompleted { get; set; }
    public List<string> MilestonesDueThisWeek { get; set; } = new();
    public int TeamActivityCount { get; set; }
}
