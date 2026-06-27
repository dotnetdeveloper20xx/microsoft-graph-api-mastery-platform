using GraphBridge.Application.Dtos.Onboarding;
using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.CreateEmployee;

/// <summary>
/// Command to create a new employee onboarding record.
/// </summary>
public class CreateEmployeeCommand : IRequest<EmployeeOnboardingDto>
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
