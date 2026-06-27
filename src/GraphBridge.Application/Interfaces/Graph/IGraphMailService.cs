using GraphBridge.Application.Dtos.Graph;

namespace GraphBridge.Application.Interfaces.Graph;

/// <summary>
/// Abstraction for Microsoft Graph Mail API operations.
/// Implementations exist for both Live (Graph SDK) and Demo (mock data) modes.
/// </summary>
public interface IGraphMailService
{
    Task SendEmailAsync(SendEmailRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<EmailSummaryDto>> GetRecentEmailsAsync(int hours = 24, CancellationToken ct = default);
    Task<EmailVolumeDto> GetEmailVolumeAsync(int days = 7, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
}
