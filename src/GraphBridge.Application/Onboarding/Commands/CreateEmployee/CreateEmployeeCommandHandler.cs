using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Domain.Entities;
using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.CreateEmployee;

/// <summary>
/// Handles creation of a new employee onboarding record.
/// Sets profileCreated=true, generates a unique GUID, and persists the entity.
/// </summary>
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeOnboardingDto>
{
    private readonly IEmployeeStore _employeeStore;

    public CreateEmployeeCommandHandler(IEmployeeStore employeeStore)
    {
        _employeeStore = employeeStore;
    }

    public async Task<EmployeeOnboardingDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new EmployeeOnboarding
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Role = request.Role,
            Department = request.Department,
            ManagerName = request.ManagerName,
            Email = request.Email,
            ProfileCreated = true
        };

        await _employeeStore.AddAsync(employee);

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
