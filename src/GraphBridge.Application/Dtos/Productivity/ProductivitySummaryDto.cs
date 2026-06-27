using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Dtos.Productivity;

/// <summary>
/// Represents a weekly productivity summary aggregating calendar, email, task, and document activity.
/// </summary>
public class ProductivitySummaryDto
{
    public List<CalendarEventDto> WeeklyEvents { get; set; } = new();
    public EmailVolumeDto EmailSummary { get; set; } = new();
    public TaskCompletionSummaryDto TaskSummary { get; set; } = new();
    public List<DocumentDto> RecentDocuments { get; set; } = new();
    public List<SectionErrorDto> UnavailableSections { get; set; } = new();
}
