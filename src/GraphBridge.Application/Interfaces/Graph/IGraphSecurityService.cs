using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Security API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphSecurityService
{
    Task<IReadOnlyList<SecuritySignalDto>> GetRecentAlertsAsync(int hours = 24, int limit = 50, CancellationToken ct = default);
}
