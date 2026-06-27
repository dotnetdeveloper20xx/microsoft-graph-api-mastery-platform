using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.Onboarding.Queries.GetEmployeeStatus;

/// <summary>
/// Handles retrieving the onboarding status for a specific employee.
/// Throws NotFoundException if the employee does not exist.
/// </summary>
public class GetEmployeeStatusQueryHandler : IRequestHandler<GetEmployeeStatusQuery, OnboardingStatusDto>
{
    private readonly IEmployeeStore _employeeStore;

    public GetEmployeeStatusQueryHandler(IEmployeeStore employeeStore)
    {
        _employeeStore = employeeStore;
    }

    public async Task<OnboardingStatusDto> Handle(GetEmployeeStatusQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeStore.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        return new OnboardingStatusDto
        {
            ProfileCreated = employee.ProfileCreated,
            GroupsAssigned = employee.GroupsAssigned,
            WelcomeEmailSent = employee.WelcomeEmailSent,
            InductionScheduled = employee.InductionScheduled
        };
    }
}
