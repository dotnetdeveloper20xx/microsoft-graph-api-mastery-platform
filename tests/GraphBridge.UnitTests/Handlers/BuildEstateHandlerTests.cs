using FluentAssertions;
using GraphBridge.Application.BuildEstate;
using GraphBridge.Application.BuildEstate.Commands.CreateProject;
using GraphBridge.Application.BuildEstate.Commands.CreateTaskBoard;
using GraphBridge.Application.BuildEstate.Commands.LaunchWorkspace;
using GraphBridge.Application.BuildEstate.Commands.NotifyDirectors;
using GraphBridge.Application.BuildEstate.Commands.ScheduleKickoff;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using Moq;

namespace GraphBridge.UnitTests.Handlers;

public class BuildEstateHandlerTests
{
    private readonly Mock<IBuildEstateProjectStore> _projectStoreMock;
    private readonly Mock<IGraphDriveService> _driveServiceMock;
    private readonly Mock<IGraphPlannerService> _plannerServiceMock;
    private readonly Mock<IGraphMailService> _mailServiceMock;
    private readonly Mock<IGraphCalendarService> _calendarServiceMock;

    public BuildEstateHandlerTests()
    {
        _projectStoreMock = new Mock<IBuildEstateProjectStore>();
        _driveServiceMock = new Mock<IGraphDriveService>();
        _plannerServiceMock = new Mock<IGraphPlannerService>();
        _mailServiceMock = new Mock<IGraphMailService>();
        _calendarServiceMock = new Mock<IGraphCalendarService>();
    }

    // --- CreateProject Handler ---

    [Fact]
    public async Task CreateProject_ShouldStoreAndReturnDto()
    {
        // Arrange
        _projectStoreMock.Setup(s => s.AddAsync(It.IsAny<BuildEstateProject>())).Returns(Task.CompletedTask);
        var handler = new CreateProjectCommandHandler(_projectStoreMock.Object);
        var command = new CreateProjectCommand
        {
            Name = "Riverside Heights",
            Location = "Manchester",
            PlanningStatus = "Approved",
            Directors = new List<string> { "director1@test.com", "director2@test.com" }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Riverside Heights");
        result.Location.Should().Be("Manchester");
        result.PlanningStatus.Should().Be("Approved");
        result.Directors.Should().HaveCount(2);
        result.WorkspaceLaunched.Should().BeFalse();
        result.TaskBoardCreated.Should().BeFalse();
        _projectStoreMock.Verify(s => s.AddAsync(It.IsAny<BuildEstateProject>()), Times.Once);
    }

    // --- LaunchWorkspace Handler ---

    [Fact]
    public async Task LaunchWorkspace_WhenNotLaunched_ShouldCreateFolderStructureAndUpdate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new BuildEstateProject
        {
            Id = projectId,
            Name = "Riverside Heights",
            Location = "Manchester",
            PlanningStatus = "Approved",
            Directors = new List<string> { "director1@test.com" },
            WorkspaceLaunched = false,
            TaskBoardCreated = false
        };
        _projectStoreMock.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _projectStoreMock.Setup(s => s.UpdateAsync(It.IsAny<BuildEstateProject>())).Returns(Task.CompletedTask);
        _driveServiceMock.Setup(d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FolderStructureDto { Name = "Root", Children = new List<FolderStructureDto>() });

        var handler = new LaunchWorkspaceCommandHandler(_projectStoreMock.Object, _driveServiceMock.Object);
        var command = new LaunchWorkspaceCommand { ProjectId = projectId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _driveServiceMock.Verify(d => d.CreateFolderStructureAsync(
            It.Is<CreateFolderStructureRequest>(r =>
                r.FolderNames.Contains("Planning Documents") &&
                r.FolderNames.Contains("Contracts") &&
                r.FolderNames.Contains("Site Reports") &&
                r.FolderNames.Contains("Financial")),
            It.IsAny<CancellationToken>()), Times.Once);
        _projectStoreMock.Verify(s => s.UpdateAsync(It.Is<BuildEstateProject>(p => p.WorkspaceLaunched == true)), Times.Once);
    }

    [Fact]
    public async Task LaunchWorkspace_WhenAlreadyLaunched_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new BuildEstateProject
        {
            Id = projectId,
            Name = "Riverside Heights",
            Location = "Manchester",
            PlanningStatus = "Approved",
            Directors = new List<string> { "director1@test.com" },
            WorkspaceLaunched = true,
            TaskBoardCreated = false
        };
        _projectStoreMock.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);

        var handler = new LaunchWorkspaceCommandHandler(_projectStoreMock.Object, _driveServiceMock.Object);
        var command = new LaunchWorkspaceCommand { ProjectId = projectId };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already exists*");
        _driveServiceMock.Verify(d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- CreateTaskBoard Handler ---

    [Fact]
    public async Task CreateTaskBoard_ShouldCallPlannerServiceAndReturnTaskBoard()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new BuildEstateProject
        {
            Id = projectId,
            Name = "Riverside Heights",
            Location = "Manchester",
            PlanningStatus = "Approved",
            Directors = new List<string> { "director1@test.com" },
            WorkspaceLaunched = true,
            TaskBoardCreated = false
        };
        _projectStoreMock.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _projectStoreMock.Setup(s => s.UpdateAsync(It.IsAny<BuildEstateProject>())).Returns(Task.CompletedTask);

        var graphTaskBoard = new TaskBoardDto
        {
            Buckets = new List<TaskBucketDto>
            {
                new() { Name = "To Do", Tasks = new List<ProjectTaskDto> { new() { Title = "Task 1", Status = "Not Started", AssignedTo = "User A" } } },
                new() { Name = "In Progress", Tasks = new List<ProjectTaskDto> { new() { Title = "Task 2", Status = "In Progress", AssignedTo = "User B" } } },
                new() { Name = "Completed", Tasks = new List<ProjectTaskDto> { new() { Title = "Task 3", Status = "Completed", AssignedTo = "User C" } } }
            }
        };
        _plannerServiceMock.Setup(p => p.CreateTaskBoardAsync(It.IsAny<CreateTaskBoardRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphTaskBoard);

        var handler = new CreateTaskBoardCommandHandler(_projectStoreMock.Object, _plannerServiceMock.Object);
        var command = new CreateTaskBoardCommand { ProjectId = projectId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Buckets.Should().HaveCount(3);
        _plannerServiceMock.Verify(p => p.CreateTaskBoardAsync(
            It.Is<CreateTaskBoardRequest>(r =>
                r.BucketNames.Contains("To Do") &&
                r.BucketNames.Contains("In Progress") &&
                r.BucketNames.Contains("Completed")),
            It.IsAny<CancellationToken>()), Times.Once);
        _projectStoreMock.Verify(s => s.UpdateAsync(It.Is<BuildEstateProject>(p => p.TaskBoardCreated == true)), Times.Once);
    }

    // --- NotifyDirectors Handler ---

    [Fact]
    public async Task NotifyDirectors_WithDirectors_ShouldSendEmailsToAllAndReturnCount()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var directors = new List<string> { "dir1@test.com", "dir2@test.com", "dir3@test.com" };
        var project = new BuildEstateProject
        {
            Id = projectId,
            Name = "Riverside Heights",
            Location = "Manchester",
            PlanningStatus = "Approved",
            Directors = directors,
            WorkspaceLaunched = true,
            TaskBoardCreated = true
        };
        _projectStoreMock.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _mailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new NotifyDirectorsCommandHandler(_projectStoreMock.Object, _mailServiceMock.Object);
        var command = new NotifyDirectorsCommand { ProjectId = projectId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(3);
        _mailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task NotifyDirectors_WithNoDirectors_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new BuildEstateProject
        {
            Id = projectId,
            Name = "Empty Project",
            Location = "London",
            PlanningStatus = "Approved",
            Directors = new List<string>(),
            WorkspaceLaunched = true,
            TaskBoardCreated = false
        };
        _projectStoreMock.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);

        var handler = new NotifyDirectorsCommandHandler(_projectStoreMock.Object, _mailServiceMock.Object);
        var command = new NotifyDirectorsCommand { ProjectId = projectId };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*director*assigned*");
        _mailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- ScheduleKickoff Handler ---

    [Fact]
    public async Task ScheduleKickoff_ShouldCreateCalendarEventWithDirectorsAsAttendees()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var directors = new List<string> { "dir1@test.com", "dir2@test.com" };
        var project = new BuildEstateProject
        {
            Id = projectId,
            Name = "Riverside Heights",
            Location = "Manchester",
            PlanningStatus = "Approved",
            Directors = directors,
            WorkspaceLaunched = true,
            TaskBoardCreated = true
        };
        _projectStoreMock.Setup(s => s.GetByIdAsync(projectId)).ReturnsAsync(project);
        _calendarServiceMock.Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalendarEventDto { Subject = "Project Kickoff", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) });

        var handler = new ScheduleKickoffCommandHandler(_projectStoreMock.Object, _calendarServiceMock.Object);
        var command = new ScheduleKickoffCommand { ProjectId = projectId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _calendarServiceMock.Verify(c => c.CreateEventAsync(
            It.Is<CreateCalendarEventRequest>(r =>
                r.Subject.Contains("Riverside Heights") &&
                r.Attendees.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
