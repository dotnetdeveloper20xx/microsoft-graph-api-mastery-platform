using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetToday;

/// <summary>
/// Query to retrieve today's calendar events for the CEO dashboard,
/// limited to a maximum of 50 events.
/// </summary>
public class GetTodayQuery : IRequest<IReadOnlyList<CalendarEventDto>>
{
}
