using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.CreateWorkspace;

/// <summary>
/// Command to create a Microsoft 365 workspace for a legal matter.
/// Creates SharePoint folder structure and Teams channel.
/// </summary>
public class CreateWorkspaceCommand : IRequest<Unit>
{
    public Guid MatterId { get; set; }
}
