namespace GraphBridge.Application.Dtos.LegalMatters;

/// <summary>
/// Represents a legal matter record.
/// </summary>
public class LegalMatterDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string MatterType { get; set; } = string.Empty;
    public string AssignedSolicitor { get; set; } = string.Empty;
    public bool WorkspaceCreated { get; set; }
    public int ParticipantCount { get; set; }
}
