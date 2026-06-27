using FluentAssertions;
using GraphBridge.Application.CeoDashboard.Queries.GetCeoOverview;
using GraphBridge.Application.CeoDashboard.Queries.GetToday;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using Microsoft.Extensions.Logging;
using Moq;

namespace GraphBridge.UnitTests.Handlers;

public class CeoDashboardHandlerTests
{
    private readonly Mock<IGraphCalendarService> _calendarService = new();
    private readonly Mock<IGraphMailService> _mailService = new();
    private readonly Mock<IGraphPlannerService> _plannerService = new();
    private readonly Mock<IGraphDriveService> _driveService = new();
    private readonly Mock<IGraphSecurityService> _securityService = new();
    private readonly Mock<ILogger<GetCeoOverviewQueryHandler>> _overviewLogger = new();
    private readonly Mock<ILogger<GetTodayQueryHandler>> _todayLogger = new();

    #region GetCeoOverviewQueryHandler - Partial Results on Service Failure

    [Fact]
    public async Task GetCeoOverview_WhenCalendarServiceFails_ReturnsPartialResultsWithUnavailableSection()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetTodayEventsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calendar service unavailable"));

        _mailService
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _plannerService
            .Setup(s => s.GetPendingTasksAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlannerTaskDto> { new() { Title = "Task 1" } });

        _driveService
            .Setup(s => s.GetPendingApprovalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto> { new() { Name = "Doc 1" } });

        _securityService
            .Setup(s => s.GetRecentAlertsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SecuritySignalDto> { new() { Title = "Alert 1" } });

        var handler = new GetCeoOverviewQueryHandler(
            _calendarService.Object, _mailService.Object, _plannerService.Object,
            _driveService.Object, _securityService.Object, _overviewLogger.Object);

        // Act
        var result = await handler.Handle(new GetCeoOverviewQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TodayMeetingsCount.Should().Be(0); // Defaulted due to failure
        result.UnreadEmailsCount.Should().Be(5);
        result.PendingTasksCount.Should().Be(1);
        result.PendingDocumentApprovalsCount.Should().Be(1);
        result.ActiveSecuritySignalsCount.Should().Be(1);
        result.UnavailableSections.Should().HaveCount(1);
        result.UnavailableSections[0].Section.Should().Be("Calendar");
    }

    [Fact]
    public async Task GetCeoOverview_WhenMultipleServicesFail_ReturnsPartialResultsWithMultipleUnavailableSections()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetTodayEventsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calendar down"));

        _mailService
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mail down"));

        _plannerService
            .Setup(s => s.GetPendingTasksAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Planner down"));

        _driveService
            .Setup(s => s.GetPendingApprovalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto> { new() { Name = "Doc 1" }, new() { Name = "Doc 2" } });

        _securityService
            .Setup(s => s.GetRecentAlertsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SecuritySignalDto>());

        var handler = new GetCeoOverviewQueryHandler(
            _calendarService.Object, _mailService.Object, _plannerService.Object,
            _driveService.Object, _securityService.Object, _overviewLogger.Object);

        // Act
        var result = await handler.Handle(new GetCeoOverviewQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TodayMeetingsCount.Should().Be(0);
        result.UnreadEmailsCount.Should().Be(0);
        result.PendingTasksCount.Should().Be(0);
        result.PendingDocumentApprovalsCount.Should().Be(2);
        result.ActiveSecuritySignalsCount.Should().Be(0);
        result.UnavailableSections.Should().HaveCount(3);
        result.UnavailableSections.Select(s => s.Section).Should()
            .Contain(new[] { "Calendar", "Email", "Tasks" });
    }

    [Fact]
    public async Task GetCeoOverview_WhenAllServicesSucceed_ReturnsFullResultsWithNoUnavailableSections()
    {
        // Arrange
        _calendarService
            .Setup(s => s.GetTodayEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEventDto>
            {
                new() { Subject = "Meeting 1" },
                new() { Subject = "Meeting 2" },
                new() { Subject = "Meeting 3" }
            });

        _mailService
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        _plannerService
            .Setup(s => s.GetPendingTasksAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlannerTaskDto>
            {
                new() { Title = "Task 1" },
                new() { Title = "Task 2" }
            });

        _driveService
            .Setup(s => s.GetPendingApprovalsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto> { new() { Name = "Doc 1" } });

        _securityService
            .Setup(s => s.GetRecentAlertsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SecuritySignalDto> { new() { Title = "Alert 1" } });

        var handler = new GetCeoOverviewQueryHandler(
            _calendarService.Object, _mailService.Object, _plannerService.Object,
            _driveService.Object, _securityService.Object, _overviewLogger.Object);

        // Act
        var result = await handler.Handle(new GetCeoOverviewQuery(), CancellationToken.None);

        // Assert
        result.TodayMeetingsCount.Should().Be(3);
        result.UnreadEmailsCount.Should().Be(12);
        result.PendingTasksCount.Should().Be(2);
        result.PendingDocumentApprovalsCount.Should().Be(1);
        result.ActiveSecuritySignalsCount.Should().Be(1);
        result.UnavailableSections.Should().BeEmpty();
    }

    #endregion

    #region GetTodayQueryHandler - Response List Capping at 50

    [Fact]
    public async Task GetToday_WhenServiceReturns60Events_ResultIsCappedAt50()
    {
        // Arrange
        var events = Enumerable.Range(1, 60)
            .Select(i => new CalendarEventDto
            {
                Subject = $"Meeting {i}",
                Start = DateTime.UtcNow.AddHours(i),
                End = DateTime.UtcNow.AddHours(i + 1)
            })
            .ToList();

        _calendarService
            .Setup(s => s.GetTodayEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var handler = new GetTodayQueryHandler(_calendarService.Object);

        // Act
        var result = await handler.Handle(new GetTodayQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(50);
        result[0].Subject.Should().Be("Meeting 1");
        result[49].Subject.Should().Be("Meeting 50");
    }

    [Fact]
    public async Task GetToday_WhenServiceReturnsLessThan50Events_ReturnsAllEvents()
    {
        // Arrange
        var events = Enumerable.Range(1, 10)
            .Select(i => new CalendarEventDto
            {
                Subject = $"Meeting {i}",
                Start = DateTime.UtcNow.AddHours(i),
                End = DateTime.UtcNow.AddHours(i + 1)
            })
            .ToList();

        _calendarService
            .Setup(s => s.GetTodayEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var handler = new GetTodayQueryHandler(_calendarService.Object);

        // Act
        var result = await handler.Handle(new GetTodayQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(10);
    }

    #endregion
}
