using GraphBridge.Application.Dtos.CeoDashboard;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphBridge.Application.CeoDashboard.Queries.GetCeoOverview;

/// <summary>
/// Handles retrieving the CEO overview by aggregating counts from multiple Graph services.
/// Individual service failures result in partial results with error indicators rather than
/// failing the entire request.
/// </summary>
public class GetCeoOverviewQueryHandler : IRequestHandler<GetCeoOverviewQuery, CeoDashboardOverviewDto>
{
    private readonly IGraphCalendarService _calendarService;
    private readonly IGraphMailService _mailService;
    private readonly IGraphPlannerService _plannerService;
    private readonly IGraphDriveService _driveService;
    private readonly IGraphSecurityService _securityService;
    private readonly ILogger<GetCeoOverviewQueryHandler> _logger;

    public GetCeoOverviewQueryHandler(
        IGraphCalendarService calendarService,
        IGraphMailService mailService,
        IGraphPlannerService plannerService,
        IGraphDriveService driveService,
        IGraphSecurityService securityService,
        ILogger<GetCeoOverviewQueryHandler> logger)
    {
        _calendarService = calendarService;
        _mailService = mailService;
        _plannerService = plannerService;
        _driveService = driveService;
        _securityService = securityService;
        _logger = logger;
    }

    public async Task<CeoDashboardOverviewDto> Handle(GetCeoOverviewQuery request, CancellationToken cancellationToken)
    {
        var overview = new CeoDashboardOverviewDto();

        // Calendar - Today's meetings count
        try
        {
            var todayEvents = await _calendarService.GetTodayEventsAsync(cancellationToken);
            overview.TodayMeetingsCount = todayEvents.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve today's meetings count for CEO overview");
            overview.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Calendar",
                ErrorMessage = "Unable to retrieve today's meetings"
            });
        }

        // Mail - Unread emails count
        try
        {
            overview.UnreadEmailsCount = await _mailService.GetUnreadCountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve unread email count for CEO overview");
            overview.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Email",
                ErrorMessage = "Unable to retrieve unread email count"
            });
        }

        // Planner - Pending tasks count
        try
        {
            var pendingTasks = await _plannerService.GetPendingTasksAsync(50, cancellationToken);
            overview.PendingTasksCount = pendingTasks.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve pending tasks count for CEO overview");
            overview.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Tasks",
                ErrorMessage = "Unable to retrieve pending tasks count"
            });
        }

        // Drive - Pending document approvals count
        try
        {
            var pendingApprovals = await _driveService.GetPendingApprovalsAsync(50, cancellationToken);
            overview.PendingDocumentApprovalsCount = pendingApprovals.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve pending document approvals count for CEO overview");
            overview.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Documents",
                ErrorMessage = "Unable to retrieve pending document approvals count"
            });
        }

        // Security - Active security signals count
        try
        {
            var alerts = await _securityService.GetRecentAlertsAsync(24, 50, cancellationToken);
            overview.ActiveSecuritySignalsCount = alerts.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve security signals count for CEO overview");
            overview.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Security",
                ErrorMessage = "Unable to retrieve active security signals count"
            });
        }

        return overview;
    }
}
