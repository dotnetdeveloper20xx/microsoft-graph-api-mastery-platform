namespace GraphBridge.Application.Dtos.LegalMatters;

/// <summary>
/// Request model for creating a new legal matter.
/// </summary>
public class CreateLegalMatterRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string MatterType { get; set; } = string.Empty;
    public string AssignedSolicitor { get; set; } = string.Empty;
}
