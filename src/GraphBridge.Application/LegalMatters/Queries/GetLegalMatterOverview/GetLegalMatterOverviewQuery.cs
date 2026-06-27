using GraphBridge.Application.Dtos.LegalMatters;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Queries.GetLegalMatterOverview;

/// <summary>
/// Query to retrieve all legal matters as an overview list.
/// </summary>
public class GetLegalMatterOverviewQuery : IRequest<IReadOnlyList<LegalMatterDto>>
{
}
