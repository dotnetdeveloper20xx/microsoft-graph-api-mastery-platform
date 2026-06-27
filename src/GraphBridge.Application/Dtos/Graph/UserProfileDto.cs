namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a user profile from Microsoft Graph.
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
}
