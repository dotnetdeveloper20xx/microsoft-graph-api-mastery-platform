namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Request model for creating a folder structure via Microsoft Graph.
/// </summary>
public class CreateFolderStructureRequest
{
    public string WorkspaceId { get; set; } = string.Empty;
    public List<string> FolderNames { get; set; } = new();
}
