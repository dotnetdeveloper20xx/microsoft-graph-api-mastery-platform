using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Infrastructure.Auth;
using GraphBridge.Shared.Exceptions;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphGroupService using Microsoft Graph SDK.
/// </summary>
public class LiveGraphGroupService : IGraphGroupService
{
    private readonly ITokenCacheService _tokenCache;

    public LiveGraphGroupService(ITokenCacheService tokenCache)
    {
        _tokenCache = tokenCache;
    }

    public async Task<IReadOnlyList<GroupDto>> GetGroupsForDepartmentAsync(string department, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetGroupsForDepartment", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetGroupsForDepartment", ex.Message);
        }
    }

    public async Task AssignUserToGroupsAsync(string userId, IReadOnlyList<string> groupIds, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("AssignUserToGroups", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("AssignUserToGroups", ex.Message);
        }
    }

    public async Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetUserGroups", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetUserGroups", ex.Message);
        }
    }
}
