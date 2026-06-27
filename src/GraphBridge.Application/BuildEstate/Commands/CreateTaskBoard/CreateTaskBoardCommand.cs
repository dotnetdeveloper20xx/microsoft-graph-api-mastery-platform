using GraphBridge.Application.Dtos.BuildEstate;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.CreateTaskBoard;

/// <summary>
/// Command to create a Planner-style task board for a BuildEstate project.
/// Creates a board with at least 3 buckets (To Do, In Progress, Completed) and at least 3 tasks.
/// </summary>
public class CreateTaskBoardCommand : IRequest<TaskBoardDto>
{
    public Guid ProjectId { get; set; }
}
