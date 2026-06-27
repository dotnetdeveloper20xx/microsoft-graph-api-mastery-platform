namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Activity report summary from Microsoft Graph.
/// </summary>
public class ActivityReportDto
{
    public int TotalActivities { get; set; }
    public int ActiveUsers { get; set; }
    public DateTime ReportDate { get; set; }
}
