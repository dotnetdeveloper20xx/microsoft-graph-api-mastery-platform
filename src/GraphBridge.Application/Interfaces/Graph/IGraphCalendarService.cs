using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Calendar API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphCalendarService
{
    Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarEventDto>> GetEventsForDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarEventDto>> GetTodayEventsAsync(CancellationToken ct = default);
}
