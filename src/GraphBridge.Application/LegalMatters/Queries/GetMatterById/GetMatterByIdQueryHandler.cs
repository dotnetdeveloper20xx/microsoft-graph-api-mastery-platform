using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Queries.GetMatterById;

/// <summary>
/// Handles retrieval of a single legal matter by ID.
/// Throws NotFoundException if the matter does not exist.
/// </summary>
public class GetMatterByIdQueryHandler : IRequestHandler<GetMatterByIdQuery, LegalMatterDto>
{
    private readonly ILegalMatterStore _store;

    public GetMatterByIdQueryHandler(ILegalMatterStore store)
    {
        _store = store;
    }

    public async Task<LegalMatterDto> Handle(GetMatterByIdQuery request, CancellationToken cancellationToken)
    {
        var matter = await _store.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("LegalMatter", request.Id);

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
