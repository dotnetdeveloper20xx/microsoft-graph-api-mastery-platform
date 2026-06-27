namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a Teams channel from Microsoft Graph.
/// </summary>
public class TeamChannelDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
