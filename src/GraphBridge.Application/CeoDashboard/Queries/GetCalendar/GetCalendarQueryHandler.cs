using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetCalendar;

/// <summary>
/// Handles retrieving today's calendar events using the date range API
/// (from midnight to end of day).
/// </summary>
public class GetCalendarQueryHandler : IRequestHandler<GetCalendarQuery, IReadOnlyList<CalendarEventDto>>
{
    private readonly IGraphCalendarService _calendarService;

    public GetCalendarQueryHandler(IGraphCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var startOfDay = today;
        var endOfDay = today.AddDays(1).AddTicks(-1);

        var events = await _calendarService.GetEventsForDateRangeAsync(startOfDay, endOfDay, cancellationToken);
        return events;
    }
}
