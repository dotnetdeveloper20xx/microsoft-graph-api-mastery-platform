namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a document from Microsoft Graph (SharePoint/OneDrive).
/// </summary>
public class DocumentDto
{
    public string Name { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public string Location { get; set; } = string.Empty;
}
