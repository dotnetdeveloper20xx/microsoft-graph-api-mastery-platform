using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.Productivity;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.Productivity.Commands.GenerateWeeklySummary;
using GraphBridge.Application.Productivity.Queries.GetContextPackage;
using GraphBridge.Application.Productivity.Queries.GetProductivityCalendar;
using Microsoft.Extensions.Logging;
using Moq;

namespace GraphBridge.UnitTests.Handlers;

public class ProductivityHandlerTests
{
    private readonly Mock<IGraphCalendarService> _calendarService = new();
    private readonly Mock<IGraphMailService> _mailService = new();
    private readonly Mock<IGraphPlannerService> _plannerService = new();
    private readonly Mock<IGraphDriveService> _driveService = new();
    private readonly Mock<ILogger<GenerateWeeklySummaryCommandHandler>> _summaryLogger = new();
    private readonly Mock<ILogger<GetContextPackageQueryHandler>> _contextLogger = new();

    #region GetProductivityCalendarQueryHandler - Response List Capping at 100

    [Fact]
    public async Task GetProductivityCalendar_WhenServiceReturns150Events_ResultIsCappedAt100()
    {
        // Arrange
        var events = Enumerable.Range(1, 150)
            .Select(i => new CalendarEventDto
            {
                Subject = $"Event {i}",
                Start = DateTime.UtcNow.AddHours(i),
                End = DateTime.UtcNow.AddHours(i + 1)
            })
            .ToList();

        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var handler = new GetProductivityCalendarQueryHandler(_calendarService.Object);

        // Act
        var result = await handler.Handle(new GetProductivityCalendarQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(100);
        result[0].Subject.Should().Be("Event 1");
        result[99].Subject.Should().Be("Event 100");
    }

    [Fact]
    public async Task GetProductivityCalendar_WhenServiceReturnsLessThan100Events_ReturnsAllEvents()
    {
        // Arrange
        var events = Enumerable.Range(1, 25)
            .Select(i => new CalendarEventDto
            {
                Subject = $"Event {i}",
                Start = DateTime.UtcNow.AddHours(i),
                End = DateTime.UtcNow.AddHours(i + 1)
            })
            .ToList();

        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var handler = new GetProductivityCalendarQueryHandler(_calendarService.Object);

        // Act
        var result = await handler.Handle(new GetProductivityCalendarQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(25);
    }

    #endregion

    #region GetContextPackageQueryHandler - All Four Sections Non-Null

    [Fact]
    public async Task GetContextPackage_WhenAllServicesSucceed_ReturnsAllFourSectionsNonNull()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEventDto> { new() { Subject = "Meeting" } });

        _mailService
            .Setup(s => s.GetEmailVolumeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailVolumeDto { TotalSent = 10, TotalReceived = 20 });

        _plannerService
            .Setup(s => s.GetTaskSummaryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskCompletionSummaryDto { Completed = 5, InProgress = 3 });

        _driveService
            .Setup(s => s.GetRecentDocumentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto> { new() { Name = "Report.docx" } });

        var handler = new GetContextPackageQueryHandler(
            _calendarService.Object, _mailService.Object,
            _plannerService.Object, _driveService.Object, _contextLogger.Object);

        // Act
        var result = await handler.Handle(new GetContextPackageQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Calendar.Should().NotBeNull();
        result.Emails.Should().NotBeNull();
        result.Tasks.Should().NotBeNull();
        result.Documents.Should().NotBeNull();
    }

    [Fact]
    public async Task GetContextPackage_WhenAllServicesFail_StillReturnsAllFourSectionsNonNull()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calendar unavailable"));

        _mailService
            .Setup(s => s.GetEmailVolumeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mail unavailable"));

        _plannerService
            .Setup(s => s.GetTaskSummaryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Planner unavailable"));

        _driveService
            .Setup(s => s.GetRecentDocumentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Drive unavailable"));

        var handler = new GetContextPackageQueryHandler(
            _calendarService.Object, _mailService.Object,
            _plannerService.Object, _driveService.Object, _contextLogger.Object);

        // Act
        var result = await handler.Handle(new GetContextPackageQuery(), CancellationToken.None);

        // Assert - All four sections must still be non-null (graceful degradation)
        result.Should().NotBeNull();
        result.Calendar.Should().NotBeNull();
        result.Emails.Should().NotBeNull();
        result.Tasks.Should().NotBeNull();
        result.Documents.Should().NotBeNull();
    }

    #endregion

    #region GenerateWeeklySummaryCommandHandler - Partial Results on Service Failure

    [Fact]
    public async Task GenerateWeeklySummary_WhenCalendarAndMailFail_ReturnsPartialResultsWithUnavailableSections()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calendar service failure"));

        _mailService
            .Setup(s => s.GetEmailVolumeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mail service failure"));

        _plannerService
            .Setup(s => s.GetTaskSummaryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskCompletionSummaryDto { Completed = 10, Overdue = 2, InProgress = 5 });

        _driveService
            .Setup(s => s.GetRecentDocumentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto> { new() { Name = "Doc 1" } });

        var handler = new GenerateWeeklySummaryCommandHandler(
            _calendarService.Object, _mailService.Object,
            _plannerService.Object, _driveService.Object, _summaryLogger.Object);

        // Act
        var result = await handler.Handle(new GenerateWeeklySummaryCommand(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WeeklyEvents.Should().BeEmpty(); // Calendar failed
        result.TaskSummary.Completed.Should().Be(10);
        result.TaskSummary.InProgress.Should().Be(5);
        result.RecentDocuments.Should().HaveCount(1);
        result.UnavailableSections.Should().HaveCount(2);
        result.UnavailableSections.Select(s => s.Section).Should()
            .Contain(new[] { "Calendar", "Email" });
    }

    [Fact]
    public async Task GenerateWeeklySummary_WhenAllServicesSucceed_ReturnsFullResultsWithNoUnavailableSections()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEventDto>
            {
                new() { Subject = "Standup" },
                new() { Subject = "Sprint Review" }
            });

        _mailService
            .Setup(s => s.GetEmailVolumeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailVolumeDto { TotalSent = 50, TotalReceived = 120, UnreadCount = 8 });

        _plannerService
            .Setup(s => s.GetTaskSummaryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskCompletionSummaryDto { Completed = 7, Overdue = 1, InProgress = 4 });

        _driveService
            .Setup(s => s.GetRecentDocumentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto>
            {
                new() { Name = "Proposal.docx" },
                new() { Name = "Budget.xlsx" }
            });

        var handler = new GenerateWeeklySummaryCommandHandler(
            _calendarService.Object, _mailService.Object,
            _plannerService.Object, _driveService.Object, _summaryLogger.Object);

        // Act
        var result = await handler.Handle(new GenerateWeeklySummaryCommand(), CancellationToken.None);

        // Assert
        result.WeeklyEvents.Should().HaveCount(2);
        result.EmailSummary.TotalSent.Should().Be(50);
        result.EmailSummary.TotalReceived.Should().Be(120);
        result.TaskSummary.Completed.Should().Be(7);
        result.RecentDocuments.Should().HaveCount(2);
        result.UnavailableSections.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateWeeklySummary_WhenAllServicesFail_ReturnsEmptyResultsWithAllSectionsUnavailable()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetEventsForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calendar down"));

        _mailService
            .Setup(s => s.GetEmailVolumeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mail down"));

        _plannerService
            .Setup(s => s.GetTaskSummaryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Planner down"));

        _driveService
            .Setup(s => s.GetRecentDocumentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Drive down"));

        var handler = new GenerateWeeklySummaryCommandHandler(
            _calendarService.Object, _mailService.Object,
            _plannerService.Object, _driveService.Object, _summaryLogger.Object);

        // Act
        var result = await handler.Handle(new GenerateWeeklySummaryCommand(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UnavailableSections.Should().HaveCount(4);
        result.UnavailableSections.Select(s => s.Section).Should()
            .Contain(new[] { "Calendar", "Email", "Tasks", "Documents" });
    }

    #endregion
}
