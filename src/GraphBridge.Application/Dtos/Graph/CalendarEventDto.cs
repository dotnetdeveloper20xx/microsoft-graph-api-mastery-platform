namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a calendar event from Microsoft Graph.
/// </summary>
public class CalendarEventDto
{
    public string Subject { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<string> Attendees { get; set; } = new();
}
