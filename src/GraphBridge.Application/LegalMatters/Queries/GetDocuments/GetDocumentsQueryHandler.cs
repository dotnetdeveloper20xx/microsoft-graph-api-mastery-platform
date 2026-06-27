using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Queries.GetDocuments;

/// <summary>
/// Handles retrieval of the document folder tree for a legal matter workspace.
/// Maps the Graph FolderStructureDto to MatterDocumentTreeDto.
/// </summary>
public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, MatterDocumentTreeDto>
{
    private readonly ILegalMatterStore _store;
    private readonly IGraphDriveService _driveService;

    public GetDocumentsQueryHandler(ILegalMatterStore store, IGraphDriveService driveService)
    {
        _store = store;
        _driveService = driveService;
    }

    public async Task<MatterDocumentTreeDto> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var matter = await _store.GetByIdAsync(request.MatterId)
            ?? throw new NotFoundException("LegalMatter", request.MatterId);

        var folderStructure = await _driveService.GetFolderStructureAsync(matter.Id.ToString(), cancellationToken);

        return MapToDocumentTree(folderStructure);
    }

    private static MatterDocumentTreeDto MapToDocumentTree(FolderStructureDto folder)
    {
        return new MatterDocumentTreeDto
        {
            FolderName = folder.Name,
            Children = folder.Children.Select(MapToDocumentTree).ToList()
        };
    }
}
