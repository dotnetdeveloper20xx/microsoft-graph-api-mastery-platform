using GraphBridge.Application.Dtos.BuildEstate;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Queries.GetBuildEstateOverview;

/// <summary>
/// Handles retrieving all BuildEstate projects as a list of DTOs.
/// </summary>
public class GetBuildEstateOverviewQueryHandler : IRequestHandler<GetBuildEstateOverviewQuery, IReadOnlyList<BuildEstateProjectDto>>
{
    private readonly IBuildEstateProjectStore _projectStore;

    public GetBuildEstateOverviewQueryHandler(IBuildEstateProjectStore projectStore)
    {
        _projectStore = projectStore;
    }

    public async Task<IReadOnlyList<BuildEstateProjectDto>> Handle(GetBuildEstateOverviewQuery request, CancellationToken cancellationToken)
    {
        var projects = await _projectStore.GetAllAsync();

        return projects.Select(p => new BuildEstateProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Location = p.Location,
            PlanningStatus = p.PlanningStatus,
            Directors = p.Directors,
            WorkspaceLaunched = p.WorkspaceLaunched,
            TaskBoardCreated = p.TaskBoardCreated
        }).ToList();
    }
}
