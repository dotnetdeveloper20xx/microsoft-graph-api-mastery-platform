using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.ScheduleKickoff;

/// <summary>
/// Command to schedule a project kickoff calendar event for all team members.
/// The event must be within 14 calendar days of the request.
/// </summary>
public class ScheduleKickoffCommand : IRequest<Unit>
{
    public Guid ProjectId { get; set; }
}
