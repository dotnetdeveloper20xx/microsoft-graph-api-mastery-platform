using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.CreateWorkspace;

/// <summary>
/// Handles workspace creation for a legal matter — creates SharePoint folder structure
/// (Correspondence, Contracts, Evidence, Notes) and a Teams channel named after the reference number.
/// </summary>
public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Unit>
{
    private readonly ILegalMatterStore _store;
    private readonly IGraphDriveService _driveService;
    private readonly IGraphTeamsService _teamsService;

    public CreateWorkspaceCommandHandler(
        ILegalMatterStore store,
        IGraphDriveService driveService,
        IGraphTeamsService teamsService)
    {
        _store = store;
        _driveService = driveService;
        _teamsService = teamsService;
    }

    public async Task<Unit> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var matter = await _store.GetByIdAsync(request.MatterId)
            ?? throw new NotFoundException("LegalMatter", request.MatterId);

        if (matter.WorkspaceCreated)
        {
            throw new BusinessRuleException("Workspace already exists");
        }

        // Create SharePoint folder structure
        await _driveService.CreateFolderStructureAsync(new CreateFolderStructureRequest
        {
            WorkspaceId = matter.Id.ToString(),
            FolderNames = new List<string> { "Correspondence", "Contracts", "Evidence", "Notes" }
        }, cancellationToken);

        // Create Teams channel named after the reference number
        await _teamsService.CreateChannelAsync(new CreateChannelRequest
        {
            TeamId = matter.Id.ToString(),
            DisplayName = matter.ReferenceNumber,
            Description = $"Channel for legal matter {matter.ReferenceNumber} - {matter.ClientName}"
        }, cancellationToken);

        // Update matter state
        matter.WorkspaceCreated = true;
        await _store.UpdateAsync(matter);

        return Unit.Value;
    }
}
