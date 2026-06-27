namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Summary of an email message from Microsoft Graph.
/// </summary>
public class EmailSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool IsRead { get; set; }
}
