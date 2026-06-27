using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.Productivity;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphBridge.Application.Productivity.Commands.GenerateWeeklySummary;

/// <summary>
/// Handles generating a weekly productivity summary by aggregating data from
/// calendar, mail, planner, and drive services. Individual service failures
/// result in partial results with error indicators rather than failing the entire request.
/// </summary>
public class GenerateWeeklySummaryCommandHandler : IRequestHandler<GenerateWeeklySummaryCommand, ProductivitySummaryDto>
{
    private readonly IGraphCalendarService _calendarService;
    private readonly IGraphMailService _mailService;
    private readonly IGraphPlannerService _plannerService;
    private readonly IGraphDriveService _driveService;
    private readonly ILogger<GenerateWeeklySummaryCommandHandler> _logger;

    public GenerateWeeklySummaryCommandHandler(
        IGraphCalendarService calendarService,
        IGraphMailService mailService,
        IGraphPlannerService plannerService,
        IGraphDriveService driveService,
        ILogger<GenerateWeeklySummaryCommandHandler> logger)
    {
        _calendarService = calendarService;
        _mailService = mailService;
        _plannerService = plannerService;
        _driveService = driveService;
        _logger = logger;
    }

    public async Task<ProductivitySummaryDto> Handle(GenerateWeeklySummaryCommand request, CancellationToken cancellationToken)
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
            _logger.LogWarning(ex, "Failed to retrieve calendar events for weekly summary");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Calendar",
                ErrorMessage = "Unable to retrieve calendar events for the past 7 days"
            });
        }

        // Emails - Email volume for the past 7 days
        try
        {
            summary.EmailSummary = await _mailService.GetEmailVolumeAsync(7, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve email volume for weekly summary");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Email",
                ErrorMessage = "Unable to retrieve email volume for the past 7 days"
            });
        }

        // Tasks - Task summary for the past 7 days
        try
        {
            summary.TaskSummary = await _plannerService.GetTaskSummaryAsync(7, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve task summary for weekly summary");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Tasks",
                ErrorMessage = "Unable to retrieve task summary for the past 7 days"
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
            _logger.LogWarning(ex, "Failed to retrieve recent documents for weekly summary");
            summary.UnavailableSections.Add(new SectionErrorDto
            {
                Section = "Documents",
                ErrorMessage = "Unable to retrieve recent documents for the past 7 days"
            });
        }

        return summary;
    }
}
