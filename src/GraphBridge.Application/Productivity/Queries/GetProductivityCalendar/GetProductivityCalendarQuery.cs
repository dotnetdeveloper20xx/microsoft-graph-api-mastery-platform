using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityCalendar;

/// <summary>
/// Query to retrieve calendar events for the current week (Monday to Sunday), max 100 events.
/// </summary>
public class GetProductivityCalendarQuery : IRequest<IReadOnlyList<CalendarEventDto>>
{
}
