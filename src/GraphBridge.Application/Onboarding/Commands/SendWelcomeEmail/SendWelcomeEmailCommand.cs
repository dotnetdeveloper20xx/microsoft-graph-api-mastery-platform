using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.SendWelcomeEmail;

/// <summary>
/// Command to send a welcome email to a new employee.
/// </summary>
public class SendWelcomeEmailCommand : IRequest<Unit>
{
    public Guid EmployeeId { get; set; }
}
