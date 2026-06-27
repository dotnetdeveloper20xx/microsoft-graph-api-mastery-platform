namespace GraphBridge.Domain.Entities;

public class LegalMatter
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string MatterType { get; set; } = string.Empty;
    public string AssignedSolicitor { get; set; } = string.Empty;
    public bool WorkspaceCreated { get; set; }
    public List<string> Participants { get; set; } = new();
}
