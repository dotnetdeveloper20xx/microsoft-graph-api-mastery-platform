using GraphBridge.Application.Dtos.LegalMatters;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Queries.GetDocuments;

/// <summary>
/// Query to retrieve the document folder tree structure for a legal matter workspace.
/// </summary>
public class GetDocumentsQuery : IRequest<MatterDocumentTreeDto>
{
    public Guid MatterId { get; set; }
}
