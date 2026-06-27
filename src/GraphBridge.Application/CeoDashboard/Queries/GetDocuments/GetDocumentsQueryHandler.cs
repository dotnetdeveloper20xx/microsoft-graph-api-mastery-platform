using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetDocuments;

/// <summary>
/// Handles retrieving recent documents and pending approvals, combined and capped at 50 documents.
/// </summary>
public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    private readonly IGraphDriveService _driveService;

    public GetDocumentsQueryHandler(IGraphDriveService driveService)
    {
        _driveService = driveService;
    }

    public async Task<IReadOnlyList<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var recentDocuments = await _driveService.GetRecentDocumentsAsync(7, 50, cancellationToken);
        var pendingApprovals = await _driveService.GetPendingApprovalsAsync(50, cancellationToken);

        // Combine results, deduplicate by name, and take first 50
        var combined = recentDocuments
            .Concat(pendingApprovals)
            .DistinctBy(d => d.Name)
            .Take(50)
            .ToList();

        return combined;
    }
}
