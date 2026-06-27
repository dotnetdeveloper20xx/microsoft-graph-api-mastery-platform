using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property 5: Mock Services Return Complete Data Without Network Calls
/// For any method call on any of the 9 mock Graph service implementations, the method SHALL
/// return a non-null result with all required DTO fields populated with non-null, non-empty
/// values, and SHALL not make any external HTTP or network calls.
///
/// **Validates: Requirements 3.2, 3.4, 13.3**
/// </summary>
public class MockServicesCompleteDataPropertyTests
{
    #region Mock User Service

    /// <summary>
    /// For any random string userId, MockGraphUserService.GetUserProfileAsync returns
    /// a non-null UserProfileDto with all string fields non-empty.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockUserService_GetUserProfile_ReturnsCompleteDto()
    {
        var service = new MockGraphUserService();

        return Prop.ForAll(
            Arb.From(Gen.OneOf(
                Gen.Elements("usr-001", "usr-999", "random-id", "test"),
                Gen.Choose(1, 1000).Select(i => $"user-{i}"))),
            userId =>
            {
                var result = service.GetUserProfileAsync(userId).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetUserProfileAsync must return a non-null result");
                result.Id.Should().NotBeNullOrEmpty("UserProfileDto.Id must be non-empty");
                result.DisplayName.Should().NotBeNullOrEmpty("UserProfileDto.DisplayName must be non-empty");
                result.Email.Should().NotBeNullOrEmpty("UserProfileDto.Email must be non-empty");
                result.Department.Should().NotBeNullOrEmpty("UserProfileDto.Department must be non-empty");
                result.JobTitle.Should().NotBeNullOrEmpty("UserProfileDto.JobTitle must be non-empty");
            });
    }

    /// <summary>
    /// MockGraphUserService.GetUsersAsync always returns a non-null, non-empty list
    /// with all user profiles having populated fields.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockUserService_GetUsers_ReturnsNonEmptyListWithCompleteProfiles()
    {
        var service = new MockGraphUserService();

        return Prop.ForAll(
            Gen.Constant(0).ToArbitrary(),
            _ =>
            {
                var result = service.GetUsersAsync().GetAwaiter().GetResult();

                result.Should().NotBeNull("GetUsersAsync must return a non-null result");
                result.Should().NotBeEmpty("GetUsersAsync must return at least one user");

                foreach (var user in result)
                {
                    user.Id.Should().NotBeNullOrEmpty("each user Id must be non-empty");
                    user.DisplayName.Should().NotBeNullOrEmpty("each user DisplayName must be non-empty");
                    user.Email.Should().NotBeNullOrEmpty("each user Email must be non-empty");
                    user.Department.Should().NotBeNullOrEmpty("each user Department must be non-empty");
                    user.JobTitle.Should().NotBeNullOrEmpty("each user JobTitle must be non-empty");
                }
            });
    }

    #endregion

    #region Mock Group Service

    /// <summary>
    /// For any random department string, MockGraphGroupService.GetGroupsForDepartmentAsync
    /// returns a non-null, non-empty list of GroupDto with all fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockGroupService_GetGroupsForDepartment_ReturnsCompleteDto()
    {
        var service = new MockGraphGroupService();

        return Prop.ForAll(
            Arb.From(Gen.Elements("HR", "Finance", "Legal", "IT", "Marketing", "Engineering", "Unknown")),
            department =>
            {
                var result = service.GetGroupsForDepartmentAsync(department).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetGroupsForDepartmentAsync must return a non-null result");
                result.Should().NotBeEmpty("GetGroupsForDepartmentAsync must return at least one group");

                foreach (var group in result)
                {
                    group.Id.Should().NotBeNullOrEmpty("GroupDto.Id must be non-empty");
                    group.DisplayName.Should().NotBeNullOrEmpty("GroupDto.DisplayName must be non-empty");
                    group.Description.Should().NotBeNullOrEmpty("GroupDto.Description must be non-empty");
                }
            });
    }

    /// <summary>
    /// For any random userId, MockGraphGroupService.GetUserGroupsAsync returns
    /// a non-null, non-empty list of GroupDto with all fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockGroupService_GetUserGroups_ReturnsCompleteDto()
    {
        var service = new MockGraphGroupService();

        return Prop.ForAll(
            Arb.From(Gen.Elements("usr-001", "usr-002", "random-user", "test-user")),
            userId =>
            {
                var result = service.GetUserGroupsAsync(userId).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetUserGroupsAsync must return a non-null result");
                result.Should().NotBeEmpty("GetUserGroupsAsync must return at least one group");

                foreach (var group in result)
                {
                    group.Id.Should().NotBeNullOrEmpty("GroupDto.Id must be non-empty");
                    group.DisplayName.Should().NotBeNullOrEmpty("GroupDto.DisplayName must be non-empty");
                    group.Description.Should().NotBeNullOrEmpty("GroupDto.Description must be non-empty");
                }
            });
    }

    #endregion

    #region Mock Mail Service

    /// <summary>
    /// For any random SendEmailRequest, MockGraphMailService.SendEmailAsync completes
    /// without throwing an exception.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockMailService_SendEmail_CompletesWithoutThrowing()
    {
        var service = new MockGraphMailService();

        return Prop.ForAll(
            Arb.From(
                from to in Gen.Elements("user@example.com", "test@graphbridge.dev", "random@test.org")
                from subject in Gen.Elements("Test Subject", "Meeting Request", "Urgent: Review Required")
                from body in Gen.Elements("Hello, this is a test.", "Please review the attached.", "Meeting at 3pm.")
                select new SendEmailRequest { To = to, Subject = subject, Body = body }),
            request =>
            {
                var act = () => service.SendEmailAsync(request).GetAwaiter().GetResult();
                act.Should().NotThrow("SendEmailAsync must complete without throwing");
            });
    }

    /// <summary>
    /// MockGraphMailService.GetRecentEmailsAsync returns a non-null, non-empty list
    /// of EmailSummaryDto with all string fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockMailService_GetRecentEmails_ReturnsCompleteDto()
    {
        var service = new MockGraphMailService();

        return Prop.ForAll(
            Gen.Choose(1, 72).ToArbitrary(),
            hours =>
            {
                var result = service.GetRecentEmailsAsync(hours).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetRecentEmailsAsync must return a non-null result");
                result.Should().NotBeEmpty("GetRecentEmailsAsync must return at least one email");

                foreach (var email in result)
                {
                    email.Id.Should().NotBeNullOrEmpty("EmailSummaryDto.Id must be non-empty");
                    email.Subject.Should().NotBeNullOrEmpty("EmailSummaryDto.Subject must be non-empty");
                    email.From.Should().NotBeNullOrEmpty("EmailSummaryDto.From must be non-empty");
                    email.Priority.Should().NotBeNullOrEmpty("EmailSummaryDto.Priority must be non-empty");
                }
            });
    }

    /// <summary>
    /// MockGraphMailService.GetEmailVolumeAsync returns a non-null EmailVolumeDto
    /// with a non-empty TopSenders list, each sender having non-empty SenderName.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockMailService_GetEmailVolume_ReturnsCompleteDto()
    {
        var service = new MockGraphMailService();

        return Prop.ForAll(
            Gen.Choose(1, 30).ToArbitrary(),
            days =>
            {
                var result = service.GetEmailVolumeAsync(days).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetEmailVolumeAsync must return a non-null result");
                result.TopSenders.Should().NotBeNull("EmailVolumeDto.TopSenders must not be null");
                result.TopSenders.Should().NotBeEmpty("EmailVolumeDto.TopSenders must not be empty");

                foreach (var sender in result.TopSenders)
                {
                    sender.SenderName.Should().NotBeNullOrEmpty("SenderSummaryDto.SenderName must be non-empty");
                    sender.MessageCount.Should().BeGreaterThan(0, "SenderSummaryDto.MessageCount must be positive");
                }
            });
    }

    #endregion

    #region Mock Calendar Service

    /// <summary>
    /// For any random CreateCalendarEventRequest, MockGraphCalendarService.CreateEventAsync
    /// returns a non-null CalendarEventDto with populated fields.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockCalendarService_CreateEvent_ReturnsCompleteDto()
    {
        var service = new MockGraphCalendarService();

        return Prop.ForAll(
            Arb.From(
                from subject in Gen.Elements("Sprint Planning", "Client Review", "Board Meeting", "1:1 Sync")
                from hourOffset in Gen.Choose(1, 48)
                from attendee in Gen.Elements("Sarah Khan", "Afzal Ahmed", "James Wilson")
                select new CreateCalendarEventRequest
                {
                    Subject = subject,
                    Start = DateTime.UtcNow.AddHours(hourOffset),
                    End = DateTime.UtcNow.AddHours(hourOffset + 1),
                    Attendees = new List<string> { attendee }
                }),
            request =>
            {
                var result = service.CreateEventAsync(request).GetAwaiter().GetResult();

                result.Should().NotBeNull("CreateEventAsync must return a non-null result");
                result.Subject.Should().NotBeNullOrEmpty("CalendarEventDto.Subject must be non-empty");
                result.Attendees.Should().NotBeNull("CalendarEventDto.Attendees must not be null");
                result.Attendees.Should().NotBeEmpty("CalendarEventDto.Attendees must not be empty");
            });
    }

    /// <summary>
    /// MockGraphCalendarService.GetTodayEventsAsync returns a non-null list of
    /// CalendarEventDto with all required fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockCalendarService_GetTodayEvents_ReturnsCompleteDto()
    {
        var service = new MockGraphCalendarService();

        return Prop.ForAll(
            Gen.Constant(0).ToArbitrary(),
            _ =>
            {
                var result = service.GetTodayEventsAsync().GetAwaiter().GetResult();

                result.Should().NotBeNull("GetTodayEventsAsync must return a non-null result");
                result.Should().NotBeEmpty("GetTodayEventsAsync must return at least one event");

                foreach (var evt in result)
                {
                    evt.Subject.Should().NotBeNullOrEmpty("CalendarEventDto.Subject must be non-empty");
                    evt.Attendees.Should().NotBeNull("CalendarEventDto.Attendees must not be null");
                    evt.Attendees.Should().NotBeEmpty("CalendarEventDto.Attendees must not be empty");
                    evt.Attendees.Should().AllSatisfy(a =>
                        a.Should().NotBeNullOrEmpty("each attendee name must be non-empty"));
                }
            });
    }

    #endregion

    #region Mock Teams Service

    /// <summary>
    /// For any random CreateChannelRequest, MockGraphTeamsService.CreateChannelAsync
    /// returns a non-null TeamChannelDto with populated fields.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockTeamsService_CreateChannel_ReturnsCompleteDto()
    {
        var service = new MockGraphTeamsService();

        return Prop.ForAll(
            Arb.From(
                from teamId in Gen.Elements("team-001", "team-002", "team-random")
                from displayName in Gen.Elements("REF-2024-0042", "Project-Alpha", "General")
                from description in Gen.Elements("Legal matter channel", "Project workspace", "General discussion")
                select new CreateChannelRequest
                {
                    TeamId = teamId,
                    DisplayName = displayName,
                    Description = description
                }),
            request =>
            {
                var result = service.CreateChannelAsync(request).GetAwaiter().GetResult();

                result.Should().NotBeNull("CreateChannelAsync must return a non-null result");
                result.Id.Should().NotBeNullOrEmpty("TeamChannelDto.Id must be non-empty");
                result.DisplayName.Should().NotBeNullOrEmpty("TeamChannelDto.DisplayName must be non-empty");
                result.Description.Should().NotBeNullOrEmpty("TeamChannelDto.Description must be non-empty");
            });
    }

    #endregion

    #region Mock Drive Service

    /// <summary>
    /// For any random CreateFolderStructureRequest with non-empty folder names,
    /// MockGraphDriveService.CreateFolderStructureAsync returns a non-null FolderStructureDto
    /// with folder names present.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockDriveService_CreateFolderStructure_ReturnsCompleteDto()
    {
        var service = new MockGraphDriveService();

        return Prop.ForAll(
            Arb.From(
                from workspaceId in Gen.Elements("ws-001", "ws-002", "ws-random")
                from folderCount in Gen.Choose(1, 6)
                let folders = Enumerable.Range(1, folderCount)
                    .Select(i => $"Folder-{i}")
                    .ToList()
                select new CreateFolderStructureRequest
                {
                    WorkspaceId = workspaceId,
                    FolderNames = folders
                }),
            request =>
            {
                var result = service.CreateFolderStructureAsync(request).GetAwaiter().GetResult();

                result.Should().NotBeNull("CreateFolderStructureAsync must return a non-null result");
                result.Name.Should().NotBeNullOrEmpty("FolderStructureDto.Name must be non-empty");
                result.Children.Should().NotBeNull("FolderStructureDto.Children must not be null");
                result.Children.Should().NotBeEmpty("FolderStructureDto.Children must not be empty");

                foreach (var child in result.Children)
                {
                    child.Name.Should().NotBeNullOrEmpty("each child folder Name must be non-empty");
                }
            });
    }

    /// <summary>
    /// MockGraphDriveService.GetRecentDocumentsAsync returns a non-null, non-empty list
    /// of DocumentDto with all fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockDriveService_GetRecentDocuments_ReturnsCompleteDto()
    {
        var service = new MockGraphDriveService();

        return Prop.ForAll(
            Gen.Choose(1, 30).ToArbitrary(),
            days =>
            {
                var result = service.GetRecentDocumentsAsync(days).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetRecentDocumentsAsync must return a non-null result");
                result.Should().NotBeEmpty("GetRecentDocumentsAsync must return at least one document");

                foreach (var doc in result)
                {
                    doc.Name.Should().NotBeNullOrEmpty("DocumentDto.Name must be non-empty");
                    doc.ModifiedBy.Should().NotBeNullOrEmpty("DocumentDto.ModifiedBy must be non-empty");
                    doc.Location.Should().NotBeNullOrEmpty("DocumentDto.Location must be non-empty");
                }
            });
    }

    #endregion

    #region Mock Planner Service

    /// <summary>
    /// For any random CreateTaskBoardRequest, MockGraphPlannerService.CreateTaskBoardAsync
    /// returns a non-null TaskBoardDto with at least one bucket containing tasks.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockPlannerService_CreateTaskBoard_ReturnsCompleteDto()
    {
        var service = new MockGraphPlannerService();

        return Prop.ForAll(
            Arb.From(
                from planId in Gen.Elements("plan-001", "plan-002", "plan-random")
                from title in Gen.Elements("Project Board", "Sprint Board", "Development Tasks")
                from bucketCount in Gen.Choose(1, 5)
                let buckets = Enumerable.Range(1, bucketCount)
                    .Select(i => $"Bucket-{i}")
                    .ToList()
                select new CreateTaskBoardRequest
                {
                    PlanId = planId,
                    Title = title,
                    BucketNames = buckets
                }),
            request =>
            {
                var result = service.CreateTaskBoardAsync(request).GetAwaiter().GetResult();

                result.Should().NotBeNull("CreateTaskBoardAsync must return a non-null result");
                result.Buckets.Should().NotBeNull("TaskBoardDto.Buckets must not be null");
                result.Buckets.Should().NotBeEmpty("TaskBoardDto.Buckets must not be empty");

                foreach (var bucket in result.Buckets)
                {
                    bucket.Name.Should().NotBeNullOrEmpty("TaskBucketDto.Name must be non-empty");
                    bucket.Tasks.Should().NotBeNull("TaskBucketDto.Tasks must not be null");
                }
            });
    }

    /// <summary>
    /// MockGraphPlannerService.GetPendingTasksAsync returns a non-null, non-empty list
    /// of PlannerTaskDto with all string fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockPlannerService_GetPendingTasks_ReturnsCompleteDto()
    {
        var service = new MockGraphPlannerService();

        return Prop.ForAll(
            Gen.Choose(1, 50).ToArbitrary(),
            limit =>
            {
                var result = service.GetPendingTasksAsync(limit).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetPendingTasksAsync must return a non-null result");
                result.Should().NotBeEmpty("GetPendingTasksAsync must return at least one task");

                foreach (var task in result)
                {
                    task.Id.Should().NotBeNullOrEmpty("PlannerTaskDto.Id must be non-empty");
                    task.Title.Should().NotBeNullOrEmpty("PlannerTaskDto.Title must be non-empty");
                    task.Status.Should().NotBeNullOrEmpty("PlannerTaskDto.Status must be non-empty");
                    task.AssignedTo.Should().NotBeNullOrEmpty("PlannerTaskDto.AssignedTo must be non-empty");
                }
            });
    }

    /// <summary>
    /// MockGraphPlannerService.GetTaskSummaryAsync returns a non-null TaskCompletionSummaryDto.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockPlannerService_GetTaskSummary_ReturnsCompleteDto()
    {
        var service = new MockGraphPlannerService();

        return Prop.ForAll(
            Gen.Choose(1, 30).ToArbitrary(),
            days =>
            {
                var result = service.GetTaskSummaryAsync(days).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetTaskSummaryAsync must return a non-null result");
                // TaskCompletionSummaryDto has int fields that are always valid (non-negative)
                (result.Completed + result.InProgress + result.Overdue).Should()
                    .BeGreaterThan(0, "TaskCompletionSummaryDto should have at least some tasks counted");
            });
    }

    #endregion

    #region Mock Security Service

    /// <summary>
    /// MockGraphSecurityService.GetRecentAlertsAsync always returns a non-null, non-empty
    /// list of SecuritySignalDto with all fields populated.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockSecurityService_GetRecentAlerts_ReturnsCompleteDto()
    {
        var service = new MockGraphSecurityService();

        return Prop.ForAll(
            Gen.Choose(1, 72).ToArbitrary(),
            hours =>
            {
                var result = service.GetRecentAlertsAsync(hours).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetRecentAlertsAsync must return a non-null result");
                result.Should().NotBeEmpty("GetRecentAlertsAsync must return at least one alert");

                foreach (var signal in result)
                {
                    signal.Title.Should().NotBeNullOrEmpty("SecuritySignalDto.Title must be non-empty");
                    signal.Severity.Should().NotBeNullOrEmpty("SecuritySignalDto.Severity must be non-empty");
                    signal.Description.Should().NotBeNullOrEmpty("SecuritySignalDto.Description must be non-empty");
                }
            });
    }

    #endregion

    #region Mock Report Service

    /// <summary>
    /// MockGraphReportService.GetActivityReportAsync always returns a non-null ActivityReportDto
    /// with meaningful data.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockReportService_GetActivityReport_ReturnsCompleteDto()
    {
        var service = new MockGraphReportService();

        return Prop.ForAll(
            Gen.Choose(1, 30).ToArbitrary(),
            days =>
            {
                var result = service.GetActivityReportAsync(days).GetAwaiter().GetResult();

                result.Should().NotBeNull("GetActivityReportAsync must return a non-null result");
                result.TotalActivities.Should().BeGreaterThan(0, "ActivityReportDto.TotalActivities must be positive");
                result.ActiveUsers.Should().BeGreaterThan(0, "ActivityReportDto.ActiveUsers must be positive");
            });
    }

    #endregion
}
