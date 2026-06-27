using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using Microsoft.Graph;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphReportService using Microsoft Graph SDK.
/// Wraps all SDK exceptions in GraphServiceException with operation name and failure reason.
/// </summary>
public class LiveGraphReportService : IGraphReportService
{
    private readonly GraphServiceClient _graphClient;

    public LiveGraphReportService(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<ActivityReportDto> GetActivityReportAsync(int days = 7, CancellationToken ct = default)
    {
        try
        {
            // Graph Reports API returns CSV/stream data for usage reports.
            // We use the getOffice365ActiveUserCounts report as a representative activity metric.
            var period = days switch
            {
                <= 7 => "D7",
                <= 30 => "D30",
                <= 90 => "D90",
                _ => "D180"
            };

            var reportStream = await _graphClient.Reports
                .GetOffice365ActiveUserCountsWithPeriod(period)
                .GetAsync(cancellationToken: ct);

            // Parse the CSV stream for activity data
            var activeUsers = 0;
            var totalActivities = 0;

            if (reportStream != null)
            {
                using var reader = new StreamReader(reportStream);
                var content = await reader.ReadToEndAsync(ct);
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                // CSV format: Report Refresh Date, Office 365, Exchange, OneDrive, SharePoint, ...
                if (lines.Length > 1)
                {
                    var lastLine = lines[^1];
                    var columns = lastLine.Split(',');
                    if (columns.Length > 1 && int.TryParse(columns[1], out var office365Users))
                    {
                        activeUsers = office365Users;
                    }
                    // Sum up all service columns for total activities
                    for (int i = 1; i < columns.Length; i++)
                    {
                        if (int.TryParse(columns[i], out var val))
                        {
                            totalActivities += val;
                        }
                    }
                }
            }

            return new ActivityReportDto
            {
                TotalActivities = totalActivities,
                ActiveUsers = activeUsers,
                ReportDate = DateTime.UtcNow
            };
        }
        catch (Exception ex) when (ex is not GraphServiceException)
        {
            throw new GraphServiceException("GetActivityReport", ex.Message, ex);
        }
    }
}
