namespace GraphBridge.Application.Dtos.LegalMatters;

/// <summary>
/// Represents a document folder tree structure for a legal matter workspace.
/// </summary>
public class MatterDocumentTreeDto
{
    public string FolderName { get; set; } = string.Empty;
    public List<MatterDocumentTreeDto> Children { get; set; } = new();
}
