namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a folder structure from Microsoft Graph (SharePoint/OneDrive).
/// </summary>
public class FolderStructureDto
{
    public string Name { get; set; } = string.Empty;
    public List<FolderStructureDto> Children { get; set; } = new();
}
