using GraphBridge.Application.Dtos.BuildEstate;
using GraphBridge.Domain.Entities;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.CreateProject;

/// <summary>
/// Handles creation of a new BuildEstate project.
/// Generates a unique GUID, stores the project with directors, and returns the DTO.
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, BuildEstateProjectDto>
{
    private readonly IBuildEstateProjectStore _projectStore;

    public CreateProjectCommandHandler(IBuildEstateProjectStore projectStore)
    {
        _projectStore = projectStore;
    }

    public async Task<BuildEstateProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new BuildEstateProject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Location = request.Location,
            PlanningStatus = request.PlanningStatus,
            Directors = request.Directors,
            WorkspaceLaunched = false,
            TaskBoardCreated = false
        };

        await _projectStore.AddAsync(project);

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
