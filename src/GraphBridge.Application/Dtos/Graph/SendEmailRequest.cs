namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for sending an email via Microsoft Graph.
/// </summary>
public class SendEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
