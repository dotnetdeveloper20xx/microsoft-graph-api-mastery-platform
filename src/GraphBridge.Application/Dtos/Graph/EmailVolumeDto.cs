namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Email volume statistics from Microsoft Graph.
/// </summary>
public class EmailVolumeDto
{
    public int TotalSent { get; set; }
    public int TotalReceived { get; set; }
    public int UnreadCount { get; set; }
    public List<SenderSummaryDto> TopSenders { get; set; } = new();
}
