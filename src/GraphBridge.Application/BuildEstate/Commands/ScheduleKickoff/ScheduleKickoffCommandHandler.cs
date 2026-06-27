using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.ScheduleKickoff;

/// <summary>
/// Handles scheduling a project kickoff calendar event.
/// Creates the event within 14 days with all directors as attendees.
/// </summary>
public class ScheduleKickoffCommandHandler : IRequestHandler<ScheduleKickoffCommand, Unit>
{
    private readonly IBuildEstateProjectStore _projectStore;
    private readonly IGraphCalendarService _calendarService;

    public ScheduleKickoffCommandHandler(IBuildEstateProjectStore projectStore, IGraphCalendarService calendarService)
    {
        _projectStore = projectStore;
        _calendarService = calendarService;
    }

    public async Task<Unit> Handle(ScheduleKickoffCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectStore.GetByIdAsync(request.ProjectId)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var start = DateTime.UtcNow.AddDays(7); // Within 14-day window
        var end = start.AddMinutes(60);

        await _calendarService.CreateEventAsync(new CreateCalendarEventRequest
        {
            Subject = $"Project Kickoff - {project.Name}",
            Start = start,
            End = end,
            Attendees = project.Directors
        }, cancellationToken);

        return Unit.Value;
    }
}
