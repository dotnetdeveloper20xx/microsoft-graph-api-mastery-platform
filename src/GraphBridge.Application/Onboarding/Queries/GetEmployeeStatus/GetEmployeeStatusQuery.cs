using GraphBridge.Application.Dtos.Onboarding;
using MediatR;

namespace GraphBridge.Application.Onboarding.Queries.GetEmployeeStatus;

/// <summary>
/// Query to retrieve the onboarding status for a specific employee.
/// </summary>
public class GetEmployeeStatusQuery : IRequest<OnboardingStatusDto>
{
    public Guid EmployeeId { get; set; }
}
