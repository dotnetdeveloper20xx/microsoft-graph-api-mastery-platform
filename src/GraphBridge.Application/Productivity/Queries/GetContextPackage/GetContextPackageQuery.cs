using GraphBridge.Application.Dtos.Productivity;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetContextPackage;

/// <summary>
/// Query to retrieve a structured AI context package containing sections for
/// calendar, emails, tasks, and documents (all non-null).
/// </summary>
public class GetContextPackageQuery : IRequest<AiContextPackageDto>
{
}
