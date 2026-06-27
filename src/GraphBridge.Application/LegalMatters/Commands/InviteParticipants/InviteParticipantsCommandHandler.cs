using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.InviteParticipants;

/// <summary>
/// Handles inviting participants to a legal matter workspace.
/// Adds participants to the matter and returns the count of participants added.
/// </summary>
public class InviteParticipantsCommandHandler : IRequestHandler<InviteParticipantsCommand, int>
{
    private readonly ILegalMatterStore _store;

    public InviteParticipantsCommandHandler(ILegalMatterStore store)
    {
        _store = store;
    }

    public async Task<int> Handle(InviteParticipantsCommand request, CancellationToken cancellationToken)
    {
        var matter = await _store.GetByIdAsync(request.MatterId)
            ?? throw new NotFoundException("LegalMatter", request.MatterId);

        matter.Participants.AddRange(request.Participants);
        await _store.UpdateAsync(matter);

        return request.Participants.Count;
    }
}
