using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetToday;

/// <summary>
/// Handles retrieving today's calendar events, capped at 50 events.
/// </summary>
public class GetTodayQueryHandler : IRequestHandler<GetTodayQuery, IReadOnlyList<CalendarEventDto>>
{
    private readonly IGraphCalendarService _calendarService;

    public GetTodayQueryHandler(IGraphCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> Handle(GetTodayQuery request, CancellationToken cancellationToken)
    {
        var events = await _calendarService.GetTodayEventsAsync(cancellationToken);
        return events.Take(50).ToList();
    }
}
