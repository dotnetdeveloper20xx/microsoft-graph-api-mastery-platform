using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using Microsoft.Graph;
using Microsoft.Graph.Models.Security;

namespace GraphBridge.Infrastructure.LiveServices;

/// <summary>
/// Live implementation of IGraphSecurityService using Microsoft Graph SDK.
/// Wraps all SDK exceptions in GraphServiceException with operation name and failure reason.
/// </summary>
public class LiveGraphSecurityService : IGraphSecurityService
{
    private readonly GraphServiceClient _graphClient;

    public LiveGraphSecurityService(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<IReadOnlyList<SecuritySignalDto>> GetRecentAlertsAsync(int hours = 24, int limit = 50, CancellationToken ct = default)
    {
        try
        {
            var sinceDate = DateTime.UtcNow.AddHours(-hours).ToString("o");

            var alertsResponse = await _graphClient.Security.Alerts_v2.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"createdDateTime ge {sinceDate}";
                config.QueryParameters.Top = limit;
                config.QueryParameters.Orderby = new[] { "createdDateTime desc" };
            }, cancellationToken: ct);

            var alerts = alertsResponse?.Value ?? new List<Alert>();

            return alerts.Select(a => new SecuritySignalDto
            {
                Title = a.Title ?? string.Empty,
                Severity = MapAlertSeverity(a.Severity),
                DetectedAt = a.CreatedDateTime?.DateTime ?? DateTime.UtcNow,
                Description = a.Description ?? string.Empty
            }).ToList().AsReadOnly();
        }
        catch (Exception ex) when (ex is not GraphServiceException)
        {
            throw new GraphServiceException("GetRecentAlerts", ex.Message, ex);
        }
    }

    private static string MapAlertSeverity(AlertSeverity? severity) => severity switch
    {
        AlertSeverity.High => "High",
        AlertSeverity.Medium => "Medium",
        AlertSeverity.Low => "Low",
        AlertSeverity.Informational => "Informational",
        _ => "Unknown"
    };
}
