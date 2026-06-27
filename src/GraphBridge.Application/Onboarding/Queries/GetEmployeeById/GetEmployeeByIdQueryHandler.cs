using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.Onboarding.Queries.GetEmployeeById;

/// <summary>
/// Handles retrieving a single employee onboarding record by ID.
/// Throws NotFoundException if the employee does not exist.
/// </summary>
public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeOnboardingDto>
{
    private readonly IEmployeeStore _employeeStore;

    public GetEmployeeByIdQueryHandler(IEmployeeStore employeeStore)
    {
        _employeeStore = employeeStore;
    }

    public async Task<EmployeeOnboardingDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeStore.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        return new EmployeeOnboardingDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Role = employee.Role,
            Department = employee.Department,
            ManagerName = employee.ManagerName,
            Email = employee.Email,
            Status = new OnboardingStatusDto
            {
                ProfileCreated = employee.ProfileCreated,
                GroupsAssigned = employee.GroupsAssigned,
                WelcomeEmailSent = employee.WelcomeEmailSent,
                InductionScheduled = employee.InductionScheduled
            }
        };
    }
}
