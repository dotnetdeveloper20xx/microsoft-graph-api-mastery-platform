using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.LaunchWorkspace;

/// <summary>
/// Command to launch a SharePoint workspace for a BuildEstate project.
/// Creates a folder structure with Planning Documents, Contracts, Site Reports, and Financial folders.
/// </summary>
public class LaunchWorkspaceCommand : IRequest<Unit>
{
    public Guid ProjectId { get; set; }
}
