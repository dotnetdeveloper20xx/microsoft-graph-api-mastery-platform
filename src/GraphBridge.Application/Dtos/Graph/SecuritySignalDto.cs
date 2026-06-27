namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a security signal/alert from Microsoft Graph.
/// </summary>
public class SecuritySignalDto
{
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}
