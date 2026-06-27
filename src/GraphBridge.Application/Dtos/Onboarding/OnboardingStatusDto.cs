namespace GraphBridge.Application.Dtos.Onboarding;

/// <summary>
/// Represents the completion status of each onboarding step.
/// </summary>
public class OnboardingStatusDto
{
    public bool ProfileCreated { get; set; }
    public bool GroupsAssigned { get; set; }
    public bool WelcomeEmailSent { get; set; }
    public bool InductionScheduled { get; set; }
}
