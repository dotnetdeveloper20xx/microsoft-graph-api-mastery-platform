using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityCalendar;

/// <summary>
/// Handles retrieving calendar events for the current week (Monday to Sunday),
/// limited to a maximum of 100 events.
/// </summary>
public class GetProductivityCalendarQueryHandler : IRequestHandler<GetProductivityCalendarQuery, IReadOnlyList<CalendarEventDto>>
{
    private readonly IGraphCalendarService _calendarService;

    public GetProductivityCalendarQueryHandler(IGraphCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> Handle(GetProductivityCalendarQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        // Calculate Monday of the current week
        var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = today.AddDays(-daysFromMonday);

        // Sunday is the end of the current week
        var sunday = monday.AddDays(7); // End of Sunday (start of next Monday)

        var events = await _calendarService.GetEventsForDateRangeAsync(monday, sunday, cancellationToken);

        // Limit to max 100 events
        return events.Take(100).ToList();
    }
}
