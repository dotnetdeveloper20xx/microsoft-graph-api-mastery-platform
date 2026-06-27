using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.Productivity;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityOverview;

/// <summary>
/// Handles retrieving the productivity overview by aggregating data from
/// calendar, mail, planner, and drive services. Same pattern as GenerateWeeklySummary
/// but as a read-only query.
/// </summary>
public class GetProductivityOverviewQueryHandler : IRequestHandler<GetProductivityOverviewQuery, ProductivitySummaryDto>
{
    private readonly IGraphCalendarService _calendarService;
    private readonly IGraphMailService _mailService;
    private readonly IGraphPlannerService _plannerService;
    private readonly IGraphDriveService _driveService;
    private readonly ILogger<GetProductivityOverviewQueryHandler> _logger;

    public GetProductivityOverviewQueryHandler(
        IGraphCalendarService calendarService,
        IGraphMailService mailService,
        IGraphPlannerService plannerService,
        IGraphDriveService driveService,
        ILogger<GetProductivityOverviewQueryHandler> logger)
    {
        _calendarService = calendarService;
        _mailService = mailService;
        _plannerService = plannerService;
        _driveService = driveService;
        _logger = logger;
    }

    public async Task<ProductivitySummaryDto> Handle(GetProductivityOverviewQuery request, CancellationToken cancellationToken)
    {
        var summary = new ProductivitySummaryDto();
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);

        // Calendar - Events for the past 7 days
        try
        {
            var events = await _calendarService.GetEventsForDateRangeAsync(sevenDaysAgo, now, cancellationToken);
            summary.WeeklyEvents = events.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve calendar events for productivity overview");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Calendar",
                ErrorMessage = "Unable to retrieve calendar events"
            });
        }

        // Emails - Email volume for the past 7 days
        try
        {
            summary.EmailSummary = await _mailService.GetEmailVolumeAsync(7, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve email volume for productivity overview");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Email",
                ErrorMessage = "Unable to retrieve email volume"
            });
        }

        // Tasks - Task summary for the past 7 days
        try
        {
            summary.TaskSummary = await _plannerService.GetTaskSummaryAsync(7, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve task summary for productivity overview");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Tasks",
                ErrorMessage = "Unable to retrieve task summary"
            });
        }

        // Documents - Recent documents for the past 7 days (max 50)
        try
        {
            var documents = await _driveService.GetRecentDocumentsAsync(7, 50, cancellationToken);
            summary.RecentDocuments = documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve recent documents for productivity overview");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Documents",
                ErrorMessage = "Unable to retrieve recent documents"
            });
        }

        return summary;
    }
}
