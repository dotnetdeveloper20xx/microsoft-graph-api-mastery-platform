namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a Microsoft 365 group from Microsoft Graph.
/// </summary>
public class GroupDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
