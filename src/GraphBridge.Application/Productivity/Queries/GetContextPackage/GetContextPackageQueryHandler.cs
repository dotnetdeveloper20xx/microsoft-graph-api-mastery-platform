using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.Productivity;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphBridge.Application.Productivity.Queries.GetContextPackage;

/// <summary>
/// Handles retrieving the AI context package with four non-null sections:
/// calendar, emails, tasks, and documents. On failure, uses empty collections/defaults.
/// </summary>
public class GetContextPackageQueryHandler : IRequestHandler<GetContextPackageQuery, AiContextPackageDto>
{
    private readonly IGraphCalendarService _calendarService;
    private readonly IGraphMailService _mailService;
    private readonly IGraphPlannerService _plannerService;
    private readonly IGraphDriveService _driveService;
    private readonly ILogger<GetContextPackageQueryHandler> _logger;

    public GetContextPackageQueryHandler(
        IGraphCalendarService calendarService,
        IGraphMailService mailService,
        IGraphPlannerService plannerService,
        IGraphDriveService driveService,
        ILogger<GetContextPackageQueryHandler> logger)
    {
        _calendarService = calendarService;
        _mailService = mailService;
        _plannerService = plannerService;
        _driveService = driveService;
        _logger = logger;
    }

    public async Task<AiContextPackageDto> Handle(GetContextPackageQuery request, CancellationToken cancellationToken)
    {
        var contextPackage = new AiContextPackageDto();
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);

        // Calendar - Events for the past 7 days (non-null, empty list on failure)
        try
        {
            var events = await _calendarService.GetEventsForDateRangeAsync(sevenDaysAgo, now, cancellationToken);
            contextPackage.Calendar = events;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve calendar events for context package");
            contextPackage.Calendar = new List<CalendarEventDto>();
        }

        // Emails - Email volume for the past 7 days (non-null, default on failure)
        try
        {
            var emailVolume = await _mailService.GetEmailVolumeAsync(7, cancellationToken);
            contextPackage.Emails = emailVolume;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve email volume for context package");
            contextPackage.Emails = new EmailVolumeDto();
        }

        // Tasks - Task summary for the past 7 days (non-null, default on failure)
        try
        {
            var taskSummary = await _plannerService.GetTaskSummaryAsync(7, cancellationToken);
            contextPackage.Tasks = taskSummary;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve task summary for context package");
            contextPackage.Tasks = new TaskCompletionSummaryDto();
        }

        // Documents - Recent documents for the past 7 days (non-null, empty list on failure)
        try
        {
            var documents = await _driveService.GetRecentDocumentsAsync(7, 50, cancellationToken);
            contextPackage.Documents = documents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve recent documents for context package");
            contextPackage.Documents = new List<DocumentDto>();
        }

        return contextPackage;
    }
}
