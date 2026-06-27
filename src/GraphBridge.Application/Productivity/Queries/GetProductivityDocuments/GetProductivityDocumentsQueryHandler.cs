using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityDocuments;

/// <summary>
/// Handles retrieving documents accessed or modified within the past 7 days, limited to max 50.
/// </summary>
public class GetProductivityDocumentsQueryHandler : IRequestHandler<GetProductivityDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    private readonly IGraphDriveService _driveService;

    public GetProductivityDocumentsQueryHandler(IGraphDriveService driveService)
    {
        _driveService = driveService;
    }

    public async Task<IReadOnlyList<DocumentDto>> Handle(GetProductivityDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _driveService.GetRecentDocumentsAsync(7, 50, cancellationToken);
    }
}
