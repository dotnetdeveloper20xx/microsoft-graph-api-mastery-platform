using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.InviteParticipants;

/// <summary>
/// Command to invite participants (1-50) to a legal matter workspace.
/// </summary>
public class InviteParticipantsCommand : IRequest<int>
{
    public Guid MatterId { get; set; }
    public List<string> Participants { get; set; } = new();
}
