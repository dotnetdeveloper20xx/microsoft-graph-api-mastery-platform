using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Infrastructure.Auth;
using GraphBridge.Shared.Exceptions;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphUserService using Microsoft Graph SDK.
/// Requires valid Microsoft Entra ID credentials configured in Live_Mode.
/// </summary>
public class LiveGraphUserService : IGraphUserService
{
    private readonly ITokenCacheService _tokenCache;

    public LiveGraphUserService(ITokenCacheService tokenCache)
    {
        _tokenCache = tokenCache;
    }

    public async Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            // TODO: Implement with GraphServiceClient when fully wired
            throw new GraphServiceException("GetUserProfile", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetUserProfile", ex.Message);
        }
    }

    public async Task<UserProfileDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("CreateUser", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("CreateUser", ex.Message);
        }
    }

    public async Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetUsers", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetUsers", ex.Message);
        }
    }
}
