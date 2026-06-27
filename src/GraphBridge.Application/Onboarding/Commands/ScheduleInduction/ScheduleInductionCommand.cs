using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.ScheduleInduction;

/// <summary>
/// Command to schedule an induction meeting for a new employee.
/// </summary>
public class ScheduleInductionCommand : IRequest<Unit>
{
    public Guid EmployeeId { get; set; }
}
