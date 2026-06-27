using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Domain.Entities;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.CreateMatter;

/// <summary>
/// Handles creation of a new legal matter with a system-generated reference number.
/// </summary>
public class CreateMatterCommandHandler : IRequestHandler<CreateMatterCommand, LegalMatterDto>
{
    private readonly ILegalMatterStore _store;

    public CreateMatterCommandHandler(ILegalMatterStore store)
    {
        _store = store;
    }

    public async Task<LegalMatterDto> Handle(CreateMatterCommand request, CancellationToken cancellationToken)
    {
        var matter = new LegalMatter
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "LM-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            ClientName = request.ClientName,
            MatterType = request.MatterType,
            AssignedSolicitor = request.AssignedSolicitor,
            WorkspaceCreated = false,
            Participants = new List<string>()
        };

        await _store.AddAsync(matter);

        return new LegalMatterDto
        {
            Id = matter.Id,
            ReferenceNumber = matter.ReferenceNumber,
            ClientName = matter.ClientName,
            MatterType = matter.MatterType,
            AssignedSolicitor = matter.AssignedSolicitor,
            WorkspaceCreated = matter.WorkspaceCreated,
            ParticipantCount = matter.Participants.Count
        };
    }
}
