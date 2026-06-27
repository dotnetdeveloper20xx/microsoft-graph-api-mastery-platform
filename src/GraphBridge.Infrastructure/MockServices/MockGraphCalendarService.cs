using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphCalendarService for Demo_Mode.
/// Returns realistic calendar events with named attendees and business-appropriate subjects.
/// No external HTTP or network calls are made.
/// </summary>
public class MockGraphCalendarService : IGraphCalendarService
{
    public Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventRequest request, CancellationToken ct = default)
    {
        var calendarEvent = new CalendarEventDto
        {
            Subject = request.Subject,
            Start = request.Start,
            End = request.End,
            Attendees = request.Attendees.ToList()
        };

        return Task.FromResult(calendarEvent);
    }

    public Task<IReadOnlyList<CalendarEventDto>> GetEventsForDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        var baseDate = start.Date;

        var events = new List<CalendarEventDto>
        {
            new()
            {
                Subject = "Sprint Planning",
                Start = baseDate.AddHours(9),
                End = baseDate.AddHours(10),
                Attendees = new List<string> { "Afzal Ahmed", "Sarah Khan", "James Whitfield", "Priya Patel" }
            },
            new()
            {
                Subject = "Client Review - Oakfield Estates",
                Start = baseDate.AddHours(10).AddMinutes(30),
                End = baseDate.AddHours(11).AddMinutes(30),
                Attendees = new List<string> { "Emma Roberts", "Afzal Ahmed", "David Thompson" }
            },
            new()
            {
                Subject = "Team Standup",
                Start = baseDate.AddHours(11).AddMinutes(45),
                End = baseDate.AddHours(12),
                Attendees = new List<string> { "Sarah Khan", "James Whitfield", "Priya Patel", "Marcus Johnson" }
            },
            new()
            {
                Subject = "1:1 with Sarah",
                Start = baseDate.AddHours(13),
                End = baseDate.AddHours(13).AddMinutes(30),
                Attendees = new List<string> { "Afzal Ahmed", "Sarah Khan" }
            },
            new()
            {
                Subject = "Board Meeting",
                Start = baseDate.AddHours(14),
                End = baseDate.AddHours(15).AddMinutes(30),
                Attendees = new List<string> { "Afzal Ahmed", "Helen Clarke", "Richard Bennett", "Sophia Williams" }
            },
            new()
            {
                Subject = "Architecture Review",
                Start = baseDate.AddDays(1).AddHours(9).AddMinutes(30),
                End = baseDate.AddDays(1).AddHours(10).AddMinutes(30),
                Attendees = new List<string> { "Afzal Ahmed", "James Whitfield", "Marcus Johnson" }
            },
            new()
            {
                Subject = "Budget Approval Discussion",
                Start = baseDate.AddDays(1).AddHours(11),
                End = baseDate.AddDays(1).AddHours(12),
                Attendees = new List<string> { "Afzal Ahmed", "Helen Clarke", "David Thompson" }
            },
            new()
            {
                Subject = "Vendor Demo - Cloud Migration",
                Start = baseDate.AddDays(2).AddHours(14),
                End = baseDate.AddDays(2).AddHours(15),
                Attendees = new List<string> { "Afzal Ahmed", "Priya Patel", "External: Tom Harrison" }
            }
        };

        // Filter to events within the requested range
        IReadOnlyList<CalendarEventDto> filtered = events
            .Where(e => e.Start >= start && e.End <= end)
            .ToList();

        return Task.FromResult(filtered);
    }

    public Task<IReadOnlyList<CalendarEventDto>> GetTodayEventsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var events = new List<CalendarEventDto>
        {
            new()
            {
                Subject = "Team Standup",
                Start = today.AddHours(9),
                End = today.AddHours(9).AddMinutes(15),
                Attendees = new List<string> { "Afzal Ahmed", "Sarah Khan", "James Whitfield", "Priya Patel" }
            },
            new()
            {
                Subject = "Sprint Planning",
                Start = today.AddHours(10),
                End = today.AddHours(11),
                Attendees = new List<string> { "Afzal Ahmed", "Sarah Khan", "Marcus Johnson", "Emma Roberts" }
            },
            new()
            {
                Subject = "1:1 with Sarah",
                Start = today.AddHours(13),
                End = today.AddHours(13).AddMinutes(30),
                Attendees = new List<string> { "Afzal Ahmed", "Sarah Khan" }
            },
            new()
            {
                Subject = "Client Review - Greenway Holdings",
                Start = today.AddHours(14).AddMinutes(30),
                End = today.AddHours(15).AddMinutes(30),
                Attendees = new List<string> { "Afzal Ahmed", "David Thompson", "Helen Clarke" }
            },
            new()
            {
                Subject = "Board Meeting",
                Start = today.AddHours(16),
                End = today.AddHours(17),
                Attendees = new List<string> { "Afzal Ahmed", "Richard Bennett", "Sophia Williams", "Helen Clarke" }
            }
        };

        IReadOnlyList<CalendarEventDto> result = events;
        return Task.FromResult(result);
    }
}
