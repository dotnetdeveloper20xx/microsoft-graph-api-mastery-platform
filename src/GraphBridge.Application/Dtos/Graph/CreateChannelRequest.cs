namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for creating a Teams channel via Microsoft Graph.
/// </summary>
public class CreateChannelRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
