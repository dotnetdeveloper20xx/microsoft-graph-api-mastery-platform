using GraphBridge.Application.Dtos.BuildEstate;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.CreateProject;

/// <summary>
/// Command to create a new BuildEstate project.
/// </summary>
public class CreateProjectCommand : IRequest<BuildEstateProjectDto>
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PlanningStatus { get; set; } = string.Empty;
    public List<string> Directors { get; set; } = new();
}
