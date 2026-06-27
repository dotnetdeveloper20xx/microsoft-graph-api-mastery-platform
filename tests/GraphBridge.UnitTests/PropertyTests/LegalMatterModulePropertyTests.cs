using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Application.BuildEstate;
using GraphBridge.Application.BuildEstate.Commands.LaunchWorkspace;
using GraphBridge.Application.BuildEstate.Commands.NotifyDirectors;
using GraphBridge.Application.BuildEstate.Commands.ScheduleKickoff;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.LegalMatters;
using GraphBridge.Application.LegalMatters.Commands.CreateWorkspace;
using GraphBridge.Application.LegalMatters.Commands.InviteParticipants;
using GraphBridge.Application.LegalMatters.Commands.ScheduleKickoff;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using Moq;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property-based tests for the Legal Matter and BuildEstate modules (Properties 10-14).
/// Verifies workspace folder structure, idempotency guards, participant/director counts,
/// kickoff scheduling windows, and Teams channel naming.
/// </summary>
public class LegalMatterModulePropertyTests
{
    #region Generators

    private static Gen<string> NonEmptyAlphaString() =>
        Gen.Elements(
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
            "ClientA", "ClientB", "MatterX", "ProjectY", "Solicitor1", "Solicitor2",
            "Commercial", "Litigation", "Conveyancing", "Probate", "FamilyLaw"
        );

    private static Gen<string> ReferenceNumberGen() =>
        from prefix in Gen.Elements("LM", "REF", "MAT", "CASE")
        from num in Gen.Choose(1000, 99999)
        select $"{prefix}-{num}";

    private static Gen<string> EmailGen() =>
        from name in Gen.Elements("alice", "bob", "charlie", "diana", "eve", "frank", "grace", "hank")
        from domain in Gen.Elements("example.com", "test.org", "firm.co.uk", "corp.net")
        select $"{name}@{domain}";

    private static Gen<LegalMatter> LegalMatterGen(bool workspaceCreated = false) =>
        from clientName in NonEmptyAlphaString()
        from matterType in NonEmptyAlphaString()
        from solicitor in NonEmptyAlphaString()
        from refNum in ReferenceNumberGen()
        select new LegalMatter
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = refNum,
            ClientName = clientName,
            MatterType = matterType,
            AssignedSolicitor = solicitor,
            WorkspaceCreated = workspaceCreated,
            Participants = new List<string>()
        };

    private static Gen<BuildEstateProject> BuildEstateProjectGen(bool workspaceLaunched = false) =>
        from name in NonEmptyAlphaString()
        from location in NonEmptyAlphaString()
        from status in Gen.Elements("Approved", "Pending", "In Review")
        from directorCount in Gen.Choose(1, 5)
        from directors in Gen.ListOf(directorCount, EmailGen())
        select new BuildEstateProject
        {
            Id = Guid.NewGuid(),
            Name = name,
            Location = location,
            PlanningStatus = status,
            Directors = directors.ToList(),
            WorkspaceLaunched = workspaceLaunched,
            TaskBoardCreated = false
        };

    private static Gen<List<string>> ParticipantListGen(int min, int max) =>
        from count in Gen.Choose(min, max)
        from participants in Gen.ListOf(count, EmailGen())
        select participants.Distinct().Take(count).ToList() is { Count: > 0 } distinct
            ? distinct
            : participants.Take(count).ToList();

    #endregion

    #region Property 10: Workspace Folder Structure Completeness

    /// <summary>
    /// Property 10: For any legal matter, workspace creation includes all required folders
    /// (Correspondence, Contracts, Evidence, Notes).
    ///
    /// **Validates: Requirements 6.2, 8.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LegalMatter_CreateWorkspace_IncludesAllRequiredFolders()
    {
        return Prop.ForAll(
            LegalMatterGen(workspaceCreated: false).ToArbitrary(),
            matter =>
            {
                // Arrange
                var mockStore = new Mock<ILegalMatterStore>();
                mockStore.Setup(s => s.GetByIdAsync(matter.Id)).ReturnsAsync(matter);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LegalMatter>())).Returns(Task.CompletedTask);

                CreateFolderStructureRequest? capturedRequest = null;
                var mockDriveService = new Mock<IGraphDriveService>();
                mockDriveService
                    .Setup(d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<CreateFolderStructureRequest, CancellationToken>((req, _) => capturedRequest = req)
                    .ReturnsAsync(new FolderStructureDto { Name = "Root", Children = new() });

                var mockTeamsService = new Mock<IGraphTeamsService>();
                mockTeamsService
                    .Setup(t => t.CreateChannelAsync(It.IsAny<CreateChannelRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TeamChannelDto { Id = Guid.NewGuid().ToString(), DisplayName = matter.ReferenceNumber });

                var handler = new CreateWorkspaceCommandHandler(mockStore.Object, mockDriveService.Object, mockTeamsService.Object);

                // Act
                handler.Handle(new CreateWorkspaceCommand { MatterId = matter.Id }, CancellationToken.None)
                    .GetAwaiter().GetResult();

                // Assert
                capturedRequest.Should().NotBeNull();
                var requiredFolders = new[] { "Correspondence", "Contracts", "Evidence", "Notes" };
                capturedRequest!.FolderNames.Should().Contain(requiredFolders,
                    "Legal matter workspace must include all required folders");
            });
    }

    /// <summary>
    /// Property 10 (BuildEstate): For any project, workspace creation includes all required folders
    /// (Planning Documents, Contracts, Site Reports, Financial).
    ///
    /// **Validates: Requirements 6.2, 8.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property BuildEstateProject_LaunchWorkspace_IncludesAllRequiredFolders()
    {
        return Prop.ForAll(
            BuildEstateProjectGen(workspaceLaunched: false).ToArbitrary(),
            project =>
            {
                // Arrange
                var mockStore = new Mock<IBuildEstateProjectStore>();
                mockStore.Setup(s => s.GetByIdAsync(project.Id)).ReturnsAsync(project);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<BuildEstateProject>())).Returns(Task.CompletedTask);

                CreateFolderStructureRequest? capturedRequest = null;
                var mockDriveService = new Mock<IGraphDriveService>();
                mockDriveService
                    .Setup(d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<CreateFolderStructureRequest, CancellationToken>((req, _) => capturedRequest = req)
                    .ReturnsAsync(new FolderStructureDto { Name = "Root", Children = new() });

                var handler = new LaunchWorkspaceCommandHandler(mockStore.Object, mockDriveService.Object);

                // Act
                handler.Handle(new LaunchWorkspaceCommand { ProjectId = project.Id }, CancellationToken.None)
                    .GetAwaiter().GetResult();

                // Assert
                capturedRequest.Should().NotBeNull();
                var requiredFolders = new[] { "Planning Documents", "Contracts", "Site Reports", "Financial" };
                capturedRequest!.FolderNames.Should().Contain(requiredFolders,
                    "BuildEstate workspace must include all required folders");
            });
    }

    #endregion

    #region Property 11: Workspace Idempotency Guard

    /// <summary>
    /// Property 11: For any legal matter with workspace already created, re-triggering
    /// create-workspace throws BusinessRuleException without calling IGraphDriveService.
    ///
    /// **Validates: Requirements 6.4, 8.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LegalMatter_CreateWorkspaceAgain_ThrowsBusinessRuleException()
    {
        return Prop.ForAll(
            LegalMatterGen(workspaceCreated: true).ToArbitrary(),
            matter =>
            {
                // Arrange
                var mockStore = new Mock<ILegalMatterStore>();
                mockStore.Setup(s => s.GetByIdAsync(matter.Id)).ReturnsAsync(matter);

                var mockDriveService = new Mock<IGraphDriveService>();
                var mockTeamsService = new Mock<IGraphTeamsService>();

                var handler = new CreateWorkspaceCommandHandler(mockStore.Object, mockDriveService.Object, mockTeamsService.Object);

                // Act & Assert
                var act = () => handler.Handle(new CreateWorkspaceCommand { MatterId = matter.Id }, CancellationToken.None)
                    .GetAwaiter().GetResult();

                act.Should().Throw<BusinessRuleException>(
                    "Re-triggering workspace creation must throw BusinessRuleException");

                // Verify IGraphDriveService was never called
                mockDriveService.Verify(
                    d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()),
                    Times.Never,
                    "IGraphDriveService must not be called when workspace already exists");

                // Verify IGraphTeamsService was never called
                mockTeamsService.Verify(
                    t => t.CreateChannelAsync(It.IsAny<CreateChannelRequest>(), It.IsAny<CancellationToken>()),
                    Times.Never,
                    "IGraphTeamsService must not be called when workspace already exists");
            });
    }

    /// <summary>
    /// Property 11 (BuildEstate): For any project with workspace already launched, re-triggering
    /// launch-workspace throws BusinessRuleException without calling IGraphDriveService.
    ///
    /// **Validates: Requirements 6.4, 8.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property BuildEstateProject_LaunchWorkspaceAgain_ThrowsBusinessRuleException()
    {
        return Prop.ForAll(
            BuildEstateProjectGen(workspaceLaunched: true).ToArbitrary(),
            project =>
            {
                // Arrange
                var mockStore = new Mock<IBuildEstateProjectStore>();
                mockStore.Setup(s => s.GetByIdAsync(project.Id)).ReturnsAsync(project);

                var mockDriveService = new Mock<IGraphDriveService>();

                var handler = new LaunchWorkspaceCommandHandler(mockStore.Object, mockDriveService.Object);

                // Act & Assert
                var act = () => handler.Handle(new LaunchWorkspaceCommand { ProjectId = project.Id }, CancellationToken.None)
                    .GetAwaiter().GetResult();

                act.Should().Throw<BusinessRuleException>(
                    "Re-triggering workspace launch must throw BusinessRuleException");

                // Verify IGraphDriveService was never called
                mockDriveService.Verify(
                    d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()),
                    Times.Never,
                    "IGraphDriveService must not be called when workspace already launched");
            });
    }

    #endregion

    #region Property 12: Participant/Director Notification Count Accuracy

    /// <summary>
    /// Property 12: For any list of N participants (1 ≤ N ≤ 50), the invite action returns count = N.
    ///
    /// **Validates: Requirements 6.5, 8.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LegalMatter_InviteParticipants_ReturnsExactCount()
    {
        return Prop.ForAll(
            LegalMatterGen().ToArbitrary(),
            Gen.Choose(1, 50).ToArbitrary(),
            (matter, participantCount) =>
            {
                // Generate participant list of exact count
                var participants = Enumerable.Range(1, participantCount)
                    .Select(i => $"participant{i}@example.com")
                    .ToList();

                // Arrange
                var mockStore = new Mock<ILegalMatterStore>();
                mockStore.Setup(s => s.GetByIdAsync(matter.Id)).ReturnsAsync(matter);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LegalMatter>())).Returns(Task.CompletedTask);

                var handler = new InviteParticipantsCommandHandler(mockStore.Object);

                // Act
                var result = handler.Handle(
                    new InviteParticipantsCommand { MatterId = matter.Id, Participants = participants },
                    CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                result.Should().Be(participantCount,
                    $"Inviting {participantCount} participants must return count = {participantCount}");
            });
    }

    /// <summary>
    /// Property 12 (BuildEstate): For any list of N directors (1 ≤ N ≤ 20), the notify action returns count = N.
    ///
    /// **Validates: Requirements 6.5, 8.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property BuildEstateProject_NotifyDirectors_ReturnsExactCount()
    {
        return Prop.ForAll(
            Gen.Choose(1, 20).ToArbitrary(),
            directorCount =>
            {
                // Generate project with exact director count
                var directors = Enumerable.Range(1, directorCount)
                    .Select(i => $"director{i}@corp.com")
                    .ToList();

                var project = new BuildEstateProject
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Project",
                    Location = "London",
                    PlanningStatus = "Approved",
                    Directors = directors,
                    WorkspaceLaunched = false,
                    TaskBoardCreated = false
                };

                // Arrange
                var mockStore = new Mock<IBuildEstateProjectStore>();
                mockStore.Setup(s => s.GetByIdAsync(project.Id)).ReturnsAsync(project);

                var mockMailService = new Mock<IGraphMailService>();
                mockMailService
                    .Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var handler = new NotifyDirectorsCommandHandler(mockStore.Object, mockMailService.Object);

                // Act
                var result = handler.Handle(
                    new NotifyDirectorsCommand { ProjectId = project.Id },
                    CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                result.Should().Be(directorCount,
                    $"Notifying {directorCount} directors must return count = {directorCount}");

                // Verify SendEmailAsync was called once per director
                mockMailService.Verify(
                    m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()),
                    Times.Exactly(directorCount),
                    $"SendEmailAsync must be called exactly {directorCount} times");
            });
    }

    #endregion

    #region Property 13: Kickoff Scheduling Within 14-Day Window

    /// <summary>
    /// Property 13: For any legal matter, the schedule-kickoff action creates a calendar event
    /// with a start time no later than 14 calendar days from now.
    ///
    /// **Validates: Requirements 6.7, 8.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LegalMatter_ScheduleKickoff_WithinFourteenDays()
    {
        return Prop.ForAll(
            LegalMatterGen().ToArbitrary(),
            matter =>
            {
                // Add some participants
                matter.Participants = new List<string> { "alice@firm.co.uk", "bob@firm.co.uk" };

                // Arrange
                var mockStore = new Mock<ILegalMatterStore>();
                mockStore.Setup(s => s.GetByIdAsync(matter.Id)).ReturnsAsync(matter);

                CreateCalendarEventRequest? capturedRequest = null;
                var mockCalendarService = new Mock<IGraphCalendarService>();
                mockCalendarService
                    .Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<CreateCalendarEventRequest, CancellationToken>((req, _) => capturedRequest = req)
                    .ReturnsAsync(new CalendarEventDto
                    {
                        Subject = "Kickoff",
                        Start = DateTime.UtcNow.AddDays(7),
                        End = DateTime.UtcNow.AddDays(7).AddMinutes(60),
                        Attendees = new List<string>()
                    });

                var beforeRequest = DateTime.UtcNow;
                var handler = new Application.LegalMatters.Commands.ScheduleKickoff.ScheduleKickoffCommandHandler(
                    mockStore.Object, mockCalendarService.Object);

                // Act
                handler.Handle(
                    new Application.LegalMatters.Commands.ScheduleKickoff.ScheduleKickoffCommand { MatterId = matter.Id },
                    CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                capturedRequest.Should().NotBeNull("Calendar event must be created");
                capturedRequest!.Start.Should().BeAfter(beforeRequest,
                    "Kickoff event must be scheduled in the future");
                capturedRequest.Start.Should().BeBefore(beforeRequest.AddDays(14).AddMinutes(1),
                    "Kickoff event must be within 14 calendar days");
                capturedRequest.Attendees.Should().BeEquivalentTo(matter.Participants,
                    "All participants must be included as attendees");
            });
    }

    /// <summary>
    /// Property 13 (BuildEstate): For any project, the schedule-kickoff action creates a calendar event
    /// with a start time no later than 14 calendar days from now.
    ///
    /// **Validates: Requirements 6.7, 8.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property BuildEstateProject_ScheduleKickoff_WithinFourteenDays()
    {
        return Prop.ForAll(
            BuildEstateProjectGen().ToArbitrary(),
            project =>
            {
                // Arrange
                var mockStore = new Mock<IBuildEstateProjectStore>();
                mockStore.Setup(s => s.GetByIdAsync(project.Id)).ReturnsAsync(project);

                CreateCalendarEventRequest? capturedRequest = null;
                var mockCalendarService = new Mock<IGraphCalendarService>();
                mockCalendarService
                    .Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<CreateCalendarEventRequest, CancellationToken>((req, _) => capturedRequest = req)
                    .ReturnsAsync(new CalendarEventDto
                    {
                        Subject = "Project Kickoff",
                        Start = DateTime.UtcNow.AddDays(7),
                        End = DateTime.UtcNow.AddDays(7).AddMinutes(60),
                        Attendees = new List<string>()
                    });

                var beforeRequest = DateTime.UtcNow;
                var handler = new Application.BuildEstate.Commands.ScheduleKickoff.ScheduleKickoffCommandHandler(
                    mockStore.Object, mockCalendarService.Object);

                // Act
                handler.Handle(
                    new Application.BuildEstate.Commands.ScheduleKickoff.ScheduleKickoffCommand { ProjectId = project.Id },
                    CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                capturedRequest.Should().NotBeNull("Calendar event must be created");
                capturedRequest!.Start.Should().BeAfter(beforeRequest,
                    "Kickoff event must be scheduled in the future");
                capturedRequest.Start.Should().BeBefore(beforeRequest.AddDays(14).AddMinutes(1),
                    "Kickoff event must be within 14 calendar days");
                capturedRequest.Attendees.Should().BeEquivalentTo(project.Directors,
                    "All directors must be included as attendees");
            });
    }

    #endregion

    #region Property 14: Teams Channel Named After Reference

    /// <summary>
    /// Property 14: For any legal matter with a system-generated reference number,
    /// the create-workspace action creates a Teams channel whose name equals the reference number.
    ///
    /// **Validates: Requirements 6.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LegalMatter_CreateWorkspace_TeamsChannelNamedAfterReference()
    {
        return Prop.ForAll(
            LegalMatterGen(workspaceCreated: false).ToArbitrary(),
            matter =>
            {
                // Arrange
                var mockStore = new Mock<ILegalMatterStore>();
                mockStore.Setup(s => s.GetByIdAsync(matter.Id)).ReturnsAsync(matter);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LegalMatter>())).Returns(Task.CompletedTask);

                var mockDriveService = new Mock<IGraphDriveService>();
                mockDriveService
                    .Setup(d => d.CreateFolderStructureAsync(It.IsAny<CreateFolderStructureRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new FolderStructureDto { Name = "Root", Children = new() });

                CreateChannelRequest? capturedChannelRequest = null;
                var mockTeamsService = new Mock<IGraphTeamsService>();
                mockTeamsService
                    .Setup(t => t.CreateChannelAsync(It.IsAny<CreateChannelRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<CreateChannelRequest, CancellationToken>((req, _) => capturedChannelRequest = req)
                    .ReturnsAsync(new TeamChannelDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = matter.ReferenceNumber
                    });

                var handler = new CreateWorkspaceCommandHandler(mockStore.Object, mockDriveService.Object, mockTeamsService.Object);

                // Act
                handler.Handle(new CreateWorkspaceCommand { MatterId = matter.Id }, CancellationToken.None)
                    .GetAwaiter().GetResult();

                // Assert
                capturedChannelRequest.Should().NotBeNull("Teams channel must be created");
                capturedChannelRequest!.DisplayName.Should().Be(matter.ReferenceNumber,
                    "Teams channel display name must equal the matter's reference number");
            });
    }

    #endregion
}
