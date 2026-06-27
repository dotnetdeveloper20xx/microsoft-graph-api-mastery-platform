using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Dtos.CeoDashboard;

/// <summary>
/// Represents the CEO command centre overview with aggregated counts.
/// </summary>
public class CeoDashboardOverviewDto
{
    public int TodayMeetingsCount { get; set; }
    public int UnreadEmailsCount { get; set; }
    public int PendingTasksCount { get; set; }
    public int PendingDocumentApprovalsCount { get; set; }
    public int ActiveSecuritySignalsCount { get; set; }
    public List<SectionErrorDto> UnavailableSections { get; set; } = new();
}
