using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetCalendar;

/// <summary>
/// Query to retrieve today's calendar events using date range for the CEO dashboard.
/// </summary>
public class GetCalendarQuery : IRequest<IReadOnlyList<CalendarEventDto>>
{
}
