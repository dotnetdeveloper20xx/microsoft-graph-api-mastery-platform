using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetDocuments;

/// <summary>
/// Query to retrieve documents pending approval or recently modified
/// for the CEO dashboard, limited to a maximum of 50 documents.
/// </summary>
public class GetDocumentsQuery : IRequest<IReadOnlyList<DocumentDto>>
{
}
