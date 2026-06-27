using GraphBridge.Application.Dtos.BuildEstate;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Queries.GetBuildEstateOverview;

/// <summary>
/// Query to retrieve all BuildEstate projects as an overview list.
/// </summary>
public class GetBuildEstateOverviewQuery : IRequest<IReadOnlyList<BuildEstateProjectDto>>
{
}
