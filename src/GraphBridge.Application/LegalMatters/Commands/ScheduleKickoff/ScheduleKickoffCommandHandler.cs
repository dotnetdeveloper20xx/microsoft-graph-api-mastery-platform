using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LegalMatters.Commands.ScheduleKickoff;

/// <summary>
/// Handles scheduling a kickoff meeting for a legal matter.
/// Creates a calendar event within 14 days with all participants as attendees.
/// </summary>
public class ScheduleKickoffCommandHandler : IRequestHandler<ScheduleKickoffCommand, Unit>
{
    private readonly ILegalMatterStore _store;
    private readonly IGraphCalendarService _calendarService;

    public ScheduleKickoffCommandHandler(
        ILegalMatterStore store,
        IGraphCalendarService calendarService)
    {
        _store = store;
        _calendarService = calendarService;
    }

    public async Task<Unit> Handle(ScheduleKickoffCommand request, CancellationToken cancellationToken)
    {
        var matter = await _store.GetByIdAsync(request.MatterId)
            ?? throw new NotFoundException("LegalMatter", request.MatterId);

        var start = DateTime.UtcNow.AddDays(7);
        var end = start.AddMinutes(60);

        await _calendarService.CreateEventAsync(new CreateCalendarEventRequest
        {
            Subject = $"Kickoff - {matter.ReferenceNumber}",
            Start = start,
            End = end,
            Attendees = matter.Participants
        }, cancellationToken);

        return Unit.Value;
    }
}
