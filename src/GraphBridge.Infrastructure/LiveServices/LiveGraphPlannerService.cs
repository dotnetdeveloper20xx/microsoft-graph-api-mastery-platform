using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Infrastructure.Auth;
using GraphBridge.Shared.Exceptions;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphPlannerService using Microsoft Graph SDK.
/// </summary>
public class LiveGraphPlannerService : IGraphPlannerService
{
    private readonly ITokenCacheService _tokenCache;

    public LiveGraphPlannerService(ITokenCacheService tokenCache)
    {
        _tokenCache = tokenCache;
    }

    public async Task<TaskBoardDto> CreateTaskBoardAsync(CreateTaskBoardRequest request, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("CreateTaskBoard", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("CreateTaskBoard", ex.Message);
        }
    }

    public async Task<IReadOnlyList<PlannerTaskDto>> GetPendingTasksAsync(int limit = 50, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetPendingTasks", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetPendingTasks", ex.Message);
        }
    }

    public async Task<TaskCompletionSummaryDto> GetTaskSummaryAsync(int days = 7, CancellationToken ct = default)
    {
        try
        {
            var token = await _tokenCache.GetAccessTokenAsync(ct);
            throw new GraphServiceException("GetTaskSummary", "Live Graph SDK integration pending full implementation");
        }
        catch (Exception ex) when (ex is not GraphServiceException and not AuthenticationException)
        {
            throw new GraphServiceException("GetTaskSummary", ex.Message);
        }
    }
}
