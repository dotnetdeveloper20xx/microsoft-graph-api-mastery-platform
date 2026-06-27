using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphSecurityService for Demo_Mode.
/// Returns deterministic, realistic security alerts without making any network calls.
/// </summary>
public class MockGraphSecurityService : IGraphSecurityService
{
    public Task<IReadOnlyList<SecuritySignalDto>> GetRecentAlertsAsync(int hours = 24, int limit = 50, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var alerts = new List<SecuritySignalDto>
        {
            new SecuritySignalDto
            {
                Title = "Unusual sign-in activity detected",
                Severity = "High",
                DetectedAt = now.AddHours(-2),
                Description = "A sign-in was detected from an unfamiliar location (Lagos, Nigeria) for user s.khan@graphbridge.co.uk. The IP address 41.203.67.89 has not been previously associated with this account."
            },
            new SecuritySignalDto
            {
                Title = "Multiple failed login attempts",
                Severity = "Medium",
                DetectedAt = now.AddHours(-5),
                Description = "Five consecutive failed login attempts were recorded for account j.wilson@graphbridge.co.uk within a 10-minute window from IP address 185.234.72.11."
            },
            new SecuritySignalDto
            {
                Title = "New device registered",
                Severity = "Low",
                DetectedAt = now.AddHours(-8),
                Description = "A new Windows 11 device named 'DESKTOP-M7K2P' was registered to user e.chen@graphbridge.co.uk. The device was enrolled via Microsoft Intune."
            },
            new SecuritySignalDto
            {
                Title = "Suspicious email forwarding rule",
                Severity = "Medium",
                DetectedAt = now.AddHours(-12),
                Description = "An inbox rule was created on m.roberts@graphbridge.co.uk that forwards all emails containing 'invoice' or 'payment' to an external address external-collect@protonmail.com."
            }
        };

        IReadOnlyList<SecuritySignalDto> result = alerts.Take(limit).ToList();
        return Task.FromResult(result);
    }
}
