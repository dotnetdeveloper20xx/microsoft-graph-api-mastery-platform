using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph User API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphUserService
{
    Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken ct = default);
    Task<UserProfileDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(CancellationToken ct = default);
}
