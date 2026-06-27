using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.LegalMatters;
using GraphBridge.Application.LegalMatters.Commands.CreateMatter;
using GraphBridge.Application.LegalMatters.Commands.CreateWorkspace;
using GraphBridge.Application.LegalMatters.Commands.InviteParticipants;
using GraphBridge.Application.LegalMatters.Commands.ScheduleKickoff;
using GraphBridge.Application.LegalMatters.Queries.GetMatterById;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using MediatR;
using Moq;

namespace GraphBridge.UnitTests.Handlers;

public class LegalMatterHandlerTests
{
    private readonly Mock<ILegalMatterStore> _storeMock = new();

    #region CreateMatter Handler

    [Fact]
    public async Task CreateMatterHandler_ValidCommand_StoresMatterAndReturnsDto()
    {
        // Arrange
        var handler = new CreateMatterCommandHandler(_storeMock.Object);
        var command = new CreateMatterCommand
        {
            ClientName = "Oakfield Estates Ltd",
            MatterType = "Property Acquisition",
            AssignedSolicitor = "James Wilson"
        };

        _storeMock.Setup(s => s.AddAsync(It.IsAny<LegalMatter>())).Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.ReferenceNumber.Should().StartWith("LM-");
        result.ReferenceNumber.Should().HaveLength(11); // "LM-" + 8 chars
        result.ClientName.Should().Be("Oakfield Estates Ltd");
        result.MatterType.Should().Be("Property Acquisition");
        result.AssignedSolicitor.Should().Be("James Wilson");
        result.WorkspaceCreated.Should().BeFalse();
        result.ParticipantCount.Should().Be(0);

        _storeMock.Verify(s => s.AddAsync(It.Is<LegalMatter>(m =>
            m.ClientName == "Oakfield Estates Ltd" &&
            m.WorkspaceCreated == false)), Times.Once);
    }

    #endregion

    #region CreateWorkspace Handler

    [Fact]
    public async Task CreateWorkspaceHandler_MatterWithoutWorkspace_CallsDriveAndTeamsServices()
    {
        // Arrange
        var driveServiceMock = new Mock<IGraphDriveService>();
        var teamsServiceMock = new Mock<IGraphTeamsService>();
        var handler = new CreateWorkspaceCommandHandler(_storeMock.Object, driveServiceMock.Object, teamsServiceMock.Object);

        var matterId = Guid.NewGuid();
        var matter = new LegalMatter
        {
            Id = matterId,
            ReferenceNumber = "LM-ABC12345",
            ClientName = "Oakfield Estates Ltd",
            WorkspaceCreated = false,
            Participants = new List<string>()
        };

        _storeMock.Setup(s => s.GetByIdAsync(matterId)).ReturnsAsync(matter);
        _storeMock.Setup(s => s.UpdateAsync(It.IsAny<LegalMatter>())).Returns(Task.CompletedTask);
        driveServiceMock
            .Setup(d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FolderStructureDto { Name = "Root" });
        teamsServiceMock
            .Setup(t => t.CreateChannelAsync(It.IsAny<CreateChannelRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TeamChannelDto { Id = "channel-1", DisplayName = "LM-ABC12345" });

        // Act
        var result = await handler.Handle(new CreateWorkspaceCommand { MatterId = matterId }, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        driveServiceMock.Verify(d => d.CreateFolderStructureAsync(
            It.Is<CreateFolderStructureRequest>(r =>
                r.FolderNames.Contains("Correspondence") &&
                r.FolderNames.Contains("Contracts") &&
                r.FolderNames.Contains("Evidence") &&
                r.FolderNames.Contains("Notes")),
            It.IsAny<CancellationToken>()), Times.Once);
        teamsServiceMock.Verify(t => t.CreateChannelAsync(
            It.Is<CreateChannelRequest>(r =>
                r.DisplayName == "LM-ABC12345"),
            It.IsAny<CancellationToken>()), Times.Once);
        _storeMock.Verify(s => s.UpdateAsync(It.Is<LegalMatter>(m => m.WorkspaceCreated == true)), Times.Once);
    }

    [Fact]
    public async Task CreateWorkspaceHandler_MatterAlreadyHasWorkspace_ThrowsBusinessRuleException()
    {
        // Arrange
        var driveServiceMock = new Mock<IGraphDriveService>();
        var teamsServiceMock = new Mock<IGraphTeamsService>();
        var handler = new CreateWorkspaceCommandHandler(_storeMock.Object, driveServiceMock.Object, teamsServiceMock.Object);

        var matterId = Guid.NewGuid();
        var matter = new LegalMatter
        {
            Id = matterId,
            ReferenceNumber = "LM-ABC12345",
            WorkspaceCreated = true,
            Participants = new List<string>()
        };

        _storeMock.Setup(s => s.GetByIdAsync(matterId)).ReturnsAsync(matter);

        // Act
        var act = () => handler.Handle(new CreateWorkspaceCommand { MatterId = matterId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateWorkspaceHandler_NonExistentMatter_ThrowsNotFoundException()
    {
        // Arrange
        var driveServiceMock = new Mock<IGraphDriveService>();
        var teamsServiceMock = new Mock<IGraphTeamsService>();
        var handler = new CreateWorkspaceCommandHandler(_storeMock.Object, driveServiceMock.Object, teamsServiceMock.Object);
        var nonExistentId = Guid.NewGuid();

        _storeMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((LegalMatter?)null);

        // Act
        var act = () => handler.Handle(new CreateWorkspaceCommand { MatterId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region InviteParticipants Handler

    [Fact]
    public async Task InviteParticipantsHandler_ValidParticipants_ReturnsCorrectCount()
    {
        // Arrange
        var handler = new InviteParticipantsCommandHandler(_storeMock.Object);

        var matterId = Guid.NewGuid();
        var matter = new LegalMatter
        {
            Id = matterId,
            ReferenceNumber = "LM-ABC12345",
            Participants = new List<string>()
        };

        _storeMock.Setup(s => s.GetByIdAsync(matterId)).ReturnsAsync(matter);
        _storeMock.Setup(s => s.UpdateAsync(It.IsAny<LegalMatter>())).Returns(Task.CompletedTask);

        var participants = new List<string> { "alice@example.com", "bob@example.com", "charlie@example.com" };

        // Act
        var result = await handler.Handle(new InviteParticipantsCommand
        {
            MatterId = matterId,
            Participants = participants
        }, CancellationToken.None);

        // Assert
        result.Should().Be(3);
        _storeMock.Verify(s => s.UpdateAsync(It.Is<LegalMatter>(m => m.Participants.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task InviteParticipantsHandler_NonExistentMatter_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new InviteParticipantsCommandHandler(_storeMock.Object);
        var nonExistentId = Guid.NewGuid();

        _storeMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((LegalMatter?)null);

        // Act
        var act = () => handler.Handle(new InviteParticipantsCommand
        {
            MatterId = nonExistentId,
            Participants = new List<string> { "test@example.com" }
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ScheduleKickoff Handler

    [Fact]
    public async Task ScheduleKickoffHandler_ExistingMatter_CallsCalendarServiceWithParticipants()
    {
        // Arrange
        var calendarServiceMock = new Mock<IGraphCalendarService>();
        var handler = new ScheduleKickoffCommandHandler(_storeMock.Object, calendarServiceMock.Object);

        var matterId = Guid.NewGuid();
        var matter = new LegalMatter
        {
            Id = matterId,
            ReferenceNumber = "LM-ABC12345",
            Participants = new List<string> { "alice@example.com", "bob@example.com" }
        };

        _storeMock.Setup(s => s.GetByIdAsync(matterId)).ReturnsAsync(matter);
        calendarServiceMock
            .Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalendarEventDto { Subject = "Kickoff - LM-ABC12345" });

        // Act
        var result = await handler.Handle(new ScheduleKickoffCommand { MatterId = matterId }, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        calendarServiceMock.Verify(c => c.CreateEventAsync(
            It.Is<CreateCalendarEventRequest>(r =>
                r.Subject.Contains("LM-ABC12345") &&
                r.Attendees.Count == 2 &&
                r.Start <= DateTime.UtcNow.AddDays(14)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ScheduleKickoffHandler_NonExistentMatter_ThrowsNotFoundException()
    {
        // Arrange
        var calendarServiceMock = new Mock<IGraphCalendarService>();
        var handler = new ScheduleKickoffCommandHandler(_storeMock.Object, calendarServiceMock.Object);
        var nonExistentId = Guid.NewGuid();

        _storeMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((LegalMatter?)null);

        // Act
        var act = () => handler.Handle(new ScheduleKickoffCommand { MatterId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetMatterById Handler

    [Fact]
    public async Task GetMatterByIdHandler_ExistingMatter_ReturnsCorrectDto()
    {
        // Arrange
        var handler = new GetMatterByIdQueryHandler(_storeMock.Object);
        var matterId = Guid.NewGuid();
        var matter = new LegalMatter
        {
            Id = matterId,
            ReferenceNumber = "LM-XYZ99999",
            ClientName = "Oakfield Estates Ltd",
            MatterType = "Property Acquisition",
            AssignedSolicitor = "James Wilson",
            WorkspaceCreated = true,
            Participants = new List<string> { "a@b.com", "c@d.com" }
        };

        _storeMock.Setup(s => s.GetByIdAsync(matterId)).ReturnsAsync(matter);

        // Act
        var result = await handler.Handle(new GetMatterByIdQuery { Id = matterId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(matterId);
        result.ReferenceNumber.Should().Be("LM-XYZ99999");
        result.ClientName.Should().Be("Oakfield Estates Ltd");
        result.MatterType.Should().Be("Property Acquisition");
        result.AssignedSolicitor.Should().Be("James Wilson");
        result.WorkspaceCreated.Should().BeTrue();
        result.ParticipantCount.Should().Be(2);
    }

    [Fact]
    public async Task GetMatterByIdHandler_NonExistentMatter_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetMatterByIdQueryHandler(_storeMock.Object);
        var nonExistentId = Guid.NewGuid();

        _storeMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((LegalMatter?)null);

        // Act
        var act = () => handler.Handle(new GetMatterByIdQuery { Id = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
