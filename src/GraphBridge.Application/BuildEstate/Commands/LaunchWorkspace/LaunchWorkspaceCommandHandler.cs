using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.LaunchWorkspace;

/// <summary>
/// Handles launching a SharePoint workspace for a BuildEstate project.
/// Checks workspace not already launched, then creates the folder structure.
/// </summary>
public class LaunchWorkspaceCommandHandler : IRequestHandler<LaunchWorkspaceCommand, Unit>
{
    private readonly IBuildEstateProjectStore _projectStore;
    private readonly IGraphDriveService _driveService;

    public LaunchWorkspaceCommandHandler(IBuildEstateProjectStore projectStore, IGraphDriveService driveService)
    {
        _projectStore = projectStore;
        _driveService = driveService;
    }

    public async Task<Unit> Handle(LaunchWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectStore.GetByIdAsync(request.ProjectId)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (project.WorkspaceLaunched)
        {
            throw new BusinessRuleException("Workspace already exists");
        }

        await _driveService.CreateFolderStructureAsync(new CreateFolderStructureRequest
        {
            WorkspaceId = project.Id.ToString(),
            FolderNames = new List<string> { "Planning Documents", "Contracts", "Site Reports", "Financial" }
        }, cancellationToken);

        project.WorkspaceLaunched = true;
        await _projectStore.UpdateAsync(project);

        return Unit.Value;
    }
}
