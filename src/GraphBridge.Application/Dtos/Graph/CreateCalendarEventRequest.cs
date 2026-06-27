namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for creating a calendar event via Microsoft Graph.
/// </summary>
public class CreateCalendarEventRequest
{
    public string Subject { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<string> Attendees { get; set; } = new();
}
