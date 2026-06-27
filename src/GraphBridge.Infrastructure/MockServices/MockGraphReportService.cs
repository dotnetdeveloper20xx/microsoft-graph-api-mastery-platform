using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphReportService for Demo_Mode.
/// Returns deterministic, realistic activity report summaries without making any network calls.
/// </summary>
public class MockGraphReportService : IGraphReportService
{
    public Task<ActivityReportDto> GetActivityReportAsync(int days = 7, CancellationToken ct = default)
    {
        var report = new ActivityReportDto
        {
            TotalActivities = 247,
            ActiveUsers = 42,
            ReportDate = DateTime.UtcNow.Date
        };

        return Task.FromResult(report);
    }
}
