using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Group API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphGroupService
{
    Task<IReadOnlyList<GroupDto>> GetGroupsForDepartmentAsync(string department, CancellationToken ct = default);
    Task AssignUserToGroupsAsync(string userId, IReadOnlyList<string> groupIds, CancellationToken ct = default);
    Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(string userId, CancellationToken ct = default);
}
