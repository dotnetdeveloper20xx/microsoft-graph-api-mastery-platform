using GraphBridge.Application.Dtos.BuildEstate;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.CreateTaskBoard;

/// <summary>
/// Handles creating a Planner-style task board for a BuildEstate project.
/// Calls IGraphPlannerService and maps the result to a BuildEstate TaskBoardDto.
/// </summary>
public class CreateTaskBoardCommandHandler : IRequestHandler<CreateTaskBoardCommand, Dtos.BuildEstate.TaskBoardDto>
{
    private readonly IBuildEstateProjectStore _projectStore;
    private readonly IGraphPlannerService _plannerService;

    public CreateTaskBoardCommandHandler(IBuildEstateProjectStore projectStore, IGraphPlannerService plannerService)
    {
        _projectStore = projectStore;
        _plannerService = plannerService;
    }

    public async Task<Dtos.BuildEstate.TaskBoardDto> Handle(CreateTaskBoardCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectStore.GetByIdAsync(request.ProjectId)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var graphResult = await _plannerService.CreateTaskBoardAsync(new CreateTaskBoardRequest
        {
            PlanId = project.Id.ToString(),
            Title = project.Name,
            BucketNames = new List<string> { "To Do", "In Progress", "Completed" }
        }, cancellationToken);

        project.TaskBoardCreated = true;
        await _projectStore.UpdateAsync(project);

        // Map from Graph TaskBoardDto to BuildEstate TaskBoardDto
        return new Dtos.BuildEstate.TaskBoardDto
        {
            Buckets = graphResult.Buckets.Select(b => new Dtos.BuildEstate.TaskBucketDto
            {
                Name = b.Name,
                Tasks = b.Tasks.Select(t => new Dtos.BuildEstate.ProjectTaskDto
                {
                    Title = t.Title,
                    Status = t.Status,
                    AssignedTo = t.AssignedTo
                }).ToList()
            }).ToList()
        };
    }
}
