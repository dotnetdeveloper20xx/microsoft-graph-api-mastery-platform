using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Infrastructure.Auth;
using GraphBridge.Shared.Exceptions;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphCalendarService using Microsoft Graph SDK.
/// </summary>
public class LiveGraphCalendarService : IGraphCalendarService
{
    private readonly ITokenCacheService _tokenCache;

    public LiveGraphCalendarService(ITokenCacheService tokenCache)
    {
        _tokenCache = tokenCache;
    }

    public async Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("CreateEvent", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("CreateEvent", ex.Message);
        }
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetEventsForDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetEventsForDateRange", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetEventsForDateRange", ex.Message);
        }
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetTodayEventsAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetTodayEvents", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetTodayEvents", ex.Message);
        }
    }
}
