namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for sending a Teams channel notification via Microsoft Graph.
/// </summary>
public class SendChannelNotificationRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
