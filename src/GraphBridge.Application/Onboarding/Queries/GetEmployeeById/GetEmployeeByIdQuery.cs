using GraphBridge.Application.Dtos.Onboarding;
using MediatR;

namespace GraphBridge.Application.Onboarding.Queries.GetEmployeeById;

/// <summary>
/// Query to retrieve a single employee onboarding record by ID.
/// </summary>
public class GetEmployeeByIdQuery : IRequest<EmployeeOnboardingDto>
{
    public Guid EmployeeId { get; set; }
}
