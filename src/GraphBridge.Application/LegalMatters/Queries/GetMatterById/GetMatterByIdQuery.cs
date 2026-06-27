using GraphBridge.Application.Dtos.LegalMatters;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Queries.GetMatterById;

/// <summary>
/// Query to retrieve a single legal matter by its identifier.
/// </summary>
public class GetMatterByIdQuery : IRequest<LegalMatterDto>
{
    public Guid Id { get; set; }
}
