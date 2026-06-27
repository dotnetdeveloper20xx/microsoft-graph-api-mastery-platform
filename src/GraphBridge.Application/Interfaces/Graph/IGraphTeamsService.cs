using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Teams API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphTeamsService
{
    Task<TeamChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken ct = default);
    Task SendChannelNotificationAsync(SendChannelNotificationRequest request, CancellationToken ct = default);
}
