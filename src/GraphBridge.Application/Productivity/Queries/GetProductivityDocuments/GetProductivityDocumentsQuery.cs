using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityDocuments;

/// <summary>
/// Query to retrieve documents accessed or modified within the past 7 days, max 50.
/// </summary>
public class GetProductivityDocumentsQuery : IRequest<IReadOnlyList<DocumentDto>>
{
}
