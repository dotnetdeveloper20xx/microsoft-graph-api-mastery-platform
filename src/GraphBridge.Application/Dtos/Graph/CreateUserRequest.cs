namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for creating a user via Microsoft Graph.
/// </summary>
public class CreateUserRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
}
