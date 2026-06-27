using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.ScheduleKickoff;

/// <summary>
/// Command to schedule a kickoff meeting for a legal matter within 14 days,
/// including all invited participants as attendees.
/// </summary>
public class ScheduleKickoffCommand : IRequest<Unit>
{
    public Guid MatterId { get; set; }
}
