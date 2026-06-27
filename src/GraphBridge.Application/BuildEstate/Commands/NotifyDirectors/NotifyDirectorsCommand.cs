using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.NotifyDirectors;

/// <summary>
/// Command to send notification emails to all assigned directors of a BuildEstate project.
/// </summary>
public class NotifyDirectorsCommand : IRequest<int>
{
    public Guid ProjectId { get; set; }
}
