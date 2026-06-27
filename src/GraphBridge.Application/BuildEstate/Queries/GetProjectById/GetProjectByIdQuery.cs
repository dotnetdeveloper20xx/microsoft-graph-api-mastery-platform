using GraphBridge.Application.Dtos.BuildEstate;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Queries.GetProjectById;

/// <summary>
/// Query to retrieve a single BuildEstate project by its ID.
/// </summary>
public class GetProjectByIdQuery : IRequest<BuildEstateProjectDto>
{
    public Guid ProjectId { get; set; }
}
