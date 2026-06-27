using GraphBridge.Application.Dtos.LegalMatters;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Queries.GetLegalMatterOverview;

/// <summary>
/// Handles retrieval of all legal matters, mapping each to a DTO.
/// </summary>
public class GetLegalMatterOverviewQueryHandler : IRequestHandler<GetLegalMatterOverviewQuery, IReadOnlyList<LegalMatterDto>>
{
    private readonly ILegalMatterStore _store;

    public GetLegalMatterOverviewQueryHandler(ILegalMatterStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<LegalMatterDto>> Handle(GetLegalMatterOverviewQuery request, CancellationToken cancellationToken)
    {
        var matters = await _store.GetAllAsync();

        return matters.Select(m => new LegalMatterDto
        {
            Id = m.Id,
            ReferenceNumber = m.ReferenceNumber,
            ClientName = m.ClientName,
            MatterType = m.MatterType,
            AssignedSolicitor = m.AssignedSolicitor,
            WorkspaceCreated = m.WorkspaceCreated,
            ParticipantCount = m.Participants.Count
        }).ToList();
    }
}
