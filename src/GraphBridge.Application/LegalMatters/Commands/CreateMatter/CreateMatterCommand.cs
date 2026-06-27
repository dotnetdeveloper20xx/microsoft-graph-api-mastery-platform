using GraphBridge.Application.Dtos.LegalMatters;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.CreateMatter;

/// <summary>
/// Command to create a new legal matter with a system-generated reference number.
/// </summary>
public class CreateMatterCommand : IRequest<LegalMatterDto>
{
    public string ClientName { get; set; } = string.Empty;
    public string MatterType { get; set; } = string.Empty;
    public string AssignedSolicitor { get; set; } = string.Empty;
}
