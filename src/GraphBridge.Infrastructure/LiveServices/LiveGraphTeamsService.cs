using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Infrastructure.Auth;
using GraphBridge.Shared.Exceptions;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphTeamsService using Microsoft Graph SDK.
/// </summary>
public class LiveGraphTeamsService : IGraphTeamsService
{
    private readonly ITokenCacheService _tokenCache;

    public LiveGraphTeamsService(ITokenCacheService tokenCache)
    {
        _tokenCache = tokenCache;
    }

    public async Task<TeamChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("CreateChannel", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("CreateChannel", ex.Message);
        }
    }

    public async Task SendChannelNotificationAsync(SendChannelNotificationRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("SendChannelNotification", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("SendChannelNotification", ex.Message);
        }
    }
}
