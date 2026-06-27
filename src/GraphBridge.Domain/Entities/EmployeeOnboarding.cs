namespace GraphBridge.Domain.Entities;

public class EmployeeOnboarding
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool ProfileCreated { get; set; }
    public bool GroupsAssigned { get; set; }
    public bool WelcomeEmailSent { get; set; }
    public bool InductionScheduled { get; set; }
}
