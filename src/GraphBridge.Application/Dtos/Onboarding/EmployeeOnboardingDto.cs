namespace GraphBridge.Application.Dtos.Onboarding;

/// <summary>
/// Represents an employee onboarding record with current status.
/// </summary>
public class EmployeeOnboardingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public OnboardingStatusDto Status { get; set; } = new();
}
