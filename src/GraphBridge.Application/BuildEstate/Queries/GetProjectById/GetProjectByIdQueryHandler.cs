using GraphBridge.Application.Dtos.BuildEstate;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Queries.GetProjectById;

/// <summary>
/// Handles retrieving a single BuildEstate project by ID.
/// Throws NotFoundException if the project does not exist.
/// </summary>
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, BuildEstateProjectDto>
{
    private readonly IBuildEstateProjectStore _projectStore;

    public GetProjectByIdQueryHandler(IBuildEstateProjectStore projectStore)
    {
        _projectStore = projectStore;
    }

    public async Task<BuildEstateProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectStore.GetByIdAsync(request.ProjectId)
            ?? throw new NotFoundException("Project", request.ProjectId);

        return new BuildEstateProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Location = project.Location,
            PlanningStatus = project.PlanningStatus,
            Directors = project.Directors,
            WorkspaceLaunched = project.WorkspaceLaunched,
            TaskBoardCreated = project.TaskBoardCreated
        };
    }
}
