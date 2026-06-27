using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.Onboarding;
using GraphBridge.Application.Onboarding.Commands.AssignGroups;
using GraphBridge.Application.Onboarding.Commands.SendWelcomeEmail;
using GraphBridge.Application.Onboarding.Commands.ScheduleInduction;
using GraphBridge.Domain.Entities;
using Moq;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property tests for the Employee Onboarding module covering:
/// - Property 7: Department-Based Group Assignment
/// - Property 8: Welcome Email Contains Employee Identity
/// - Property 9: Induction Event Duration and Attendees
/// </summary>
public class OnboardingModulePropertyTests
{
    #region Property 7: Department-Based Group Assignment

    /// <summary>
    /// Property 7: For any valid employee with any non-empty department string, triggering the
    /// assign-groups action SHALL result in at least one group being assigned via IGraphGroupService,
    /// and the onboarding status groupsAssigned SHALL be updated to true.
    ///
    /// **Validates: Requirements 5.3, 5.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AssignGroups_ForAnyNonEmptyDepartment_AssignsAtLeastOneGroupAndUpdatesStatus()
    {
        return Prop.ForAll(
            NonEmptyDepartmentArb(),
            department =>
            {
                // Arrange
                var employeeId = Guid.NewGuid();
                var employee = new EmployeeOnboarding
                {
                    Id = employeeId,
                    Name = "Test Employee",
                    Role = "Tester",
                    Department = department,
                    ManagerName = "Test Manager",
                    Email = "test@company.com",
                    ProfileCreated = true,
                    GroupsAssigned = false
                };

                var mockStore = new Mock<IEmployeeStore>();
                mockStore.Setup(s => s.GetByIdAsync(employeeId))
                    .ReturnsAsync(employee);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<EmployeeOnboarding>()))
                    .Returns(Task.CompletedTask);

                var returnedGroups = new List<GroupDto>
                {
                    new() { Id = Guid.NewGuid().ToString(), DisplayName = $"{department} Team", Description = $"Group for {department}" }
                };

                var mockGroupService = new Mock<IGraphGroupService>();
                mockGroupService.Setup(g => g.GetGroupsForDepartmentAsync(department, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedGroups);
                mockGroupService.Setup(g => g.AssignUserToGroupsAsync(
                    It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var handler = new AssignGroupsCommandHandler(mockStore.Object, mockGroupService.Object);
                var command = new AssignGroupsCommand { EmployeeId = employeeId };

                // Act
                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — at least one group was fetched for the department
                mockGroupService.Verify(
                    g => g.GetGroupsForDepartmentAsync(department, It.IsAny<CancellationToken>()),
                    Times.Once);

                // Assert — groups were assigned to the user
                mockGroupService.Verify(
                    g => g.AssignUserToGroupsAsync(
                        employeeId.ToString(),
                        It.Is<IReadOnlyList<string>>(ids => ids.Count >= 1),
                        It.IsAny<CancellationToken>()),
                    Times.Once);

                // Assert — groupsAssigned flag is set to true
                employee.GroupsAssigned.Should().BeTrue(
                    "the assign-groups action must update groupsAssigned to true");
            });
    }

    #endregion

    #region Property 8: Welcome Email Contains Employee Identity

    /// <summary>
    /// Property 8: For any employee with a given name and role, the welcome email sent via
    /// IGraphMailService SHALL contain both the employee's name and role in the email body,
    /// and SHALL be addressed to the employee's stored email address.
    ///
    /// **Validates: Requirements 5.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SendWelcomeEmail_ForAnyNameAndRole_EmailContainsBothAndIsAddressedToEmployee()
    {
        return Prop.ForAll(
            NonEmptyNameArb(),
            NonEmptyRoleArb(),
            (name, role) =>
            {
                // Arrange
                var employeeId = Guid.NewGuid();
                var employeeEmail = $"{name.ToLower().Replace(" ", ".")}@company.com";
                var employee = new EmployeeOnboarding
                {
                    Id = employeeId,
                    Name = name,
                    Role = role,
                    Department = "IT",
                    ManagerName = "Manager Name",
                    Email = employeeEmail,
                    ProfileCreated = true,
                    WelcomeEmailSent = false
                };

                var mockStore = new Mock<IEmployeeStore>();
                mockStore.Setup(s => s.GetByIdAsync(employeeId))
                    .ReturnsAsync(employee);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<EmployeeOnboarding>()))
                    .Returns(Task.CompletedTask);

                SendEmailRequest? capturedRequest = null;
                var mockMailService = new Mock<IGraphMailService>();
                mockMailService.Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<SendEmailRequest, CancellationToken>((req, _) => capturedRequest = req)
                    .Returns(Task.CompletedTask);

                var handler = new SendWelcomeEmailCommandHandler(mockStore.Object, mockMailService.Object);
                var command = new SendWelcomeEmailCommand { EmployeeId = employeeId };

                // Act
                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — email was sent
                mockMailService.Verify(
                    m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);

                capturedRequest.Should().NotBeNull("a SendEmailRequest should have been captured");

                // Assert — email body contains employee name
                capturedRequest!.Body.Should().Contain(name,
                    "the welcome email body must contain the employee's name");

                // Assert — email body contains employee role
                capturedRequest.Body.Should().Contain(role,
                    "the welcome email body must contain the employee's role");

                // Assert — email is addressed to the employee's email
                capturedRequest.To.Should().Be(employeeEmail,
                    "the welcome email must be addressed to the employee's stored email address");

                // Assert — welcomeEmailSent flag is set to true
                employee.WelcomeEmailSent.Should().BeTrue(
                    "the send-welcome-email action must update welcomeEmailSent to true");
            });
    }

    #endregion

    #region Property 9: Induction Event Duration and Attendees

    /// <summary>
    /// Property 9: For any employee and their manager, the schedule-induction action SHALL create
    /// a calendar event with exactly 60 minutes duration, and the attendee list SHALL include both
    /// the employee and their manager.
    ///
    /// **Validates: Requirements 5.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ScheduleInduction_ForAnyEmployeeManagerPair_Creates60MinEventWithBothAsAttendees()
    {
        return Prop.ForAll(
            EmployeeManagerPairArb(),
            pair =>
            {
                var (employeeName, managerName) = pair;

                // Arrange
                var employeeId = Guid.NewGuid();
                var employeeEmail = $"{employeeName.ToLower().Replace(" ", ".")}@company.com";
                var employee = new EmployeeOnboarding
                {
                    Id = employeeId,
                    Name = employeeName,
                    Role = "Software Engineer",
                    Department = "IT",
                    ManagerName = managerName,
                    Email = employeeEmail,
                    ProfileCreated = true,
                    InductionScheduled = false
                };

                var mockStore = new Mock<IEmployeeStore>();
                mockStore.Setup(s => s.GetByIdAsync(employeeId))
                    .ReturnsAsync(employee);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<EmployeeOnboarding>()))
                    .Returns(Task.CompletedTask);

                CreateCalendarEventRequest? capturedRequest = null;
                var mockCalendarService = new Mock<IGraphCalendarService>();
                mockCalendarService.Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<CreateCalendarEventRequest, CancellationToken>((req, _) => capturedRequest = req)
                    .ReturnsAsync((CreateCalendarEventRequest req, CancellationToken _) => new CalendarEventDto
                    {
                        Subject = req.Subject,
                        Start = req.Start,
                        End = req.End,
                        Attendees = req.Attendees.ToList()
                    });

                var handler = new ScheduleInductionCommandHandler(mockStore.Object, mockCalendarService.Object);
                var command = new ScheduleInductionCommand { EmployeeId = employeeId };

                // Act
                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Assert — calendar event was created
                mockCalendarService.Verify(
                    c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);

                capturedRequest.Should().NotBeNull("a CreateCalendarEventRequest should have been captured");

                // Assert — duration is exactly 60 minutes
                var duration = capturedRequest!.End - capturedRequest.Start;
                duration.TotalMinutes.Should().Be(60,
                    "the induction event must have exactly 60 minutes duration");

                // Assert — attendees include the employee (via email)
                capturedRequest.Attendees.Should().Contain(employeeEmail,
                    "the attendee list must include the employee");

                // Assert — attendees include the manager (derived from manager name)
                var expectedManagerEmail = $"{managerName.ToLower().Replace(" ", ".")}@company.com";
                capturedRequest.Attendees.Should().Contain(expectedManagerEmail,
                    "the attendee list must include the manager");

                // Assert — inductionScheduled flag is set to true
                employee.InductionScheduled.Should().BeTrue(
                    "the schedule-induction action must update inductionScheduled to true");
            });
    }

    #endregion

    #region Generators

    /// <summary>
    /// Generates non-empty department strings (1-50 characters) representing
    /// various department names.
    /// </summary>
    private static Arbitrary<string> NonEmptyDepartmentArb()
    {
        var gen = Gen.OneOf(
            Gen.Elements(
                "HR", "Finance", "Legal", "IT", "Marketing", "Engineering",
                "Sales", "Operations", "Research", "Support", "Design",
                "Product", "QA", "Security", "Data Science"),
            Gen.Choose(1, 50).SelectMany(len =>
                Gen.ArrayOf(len, Gen.Elements(
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
                    'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
                    'u', 'v', 'w', 'x', 'y', 'z', ' '))
                .Select(chars => new string(chars).Trim())
                .Where(s => s.Length > 0)));

        return Arb.From(gen);
    }

    /// <summary>
    /// Generates non-empty name strings (1-100 characters) representing employee names.
    /// </summary>
    private static Arbitrary<string> NonEmptyNameArb()
    {
        var gen = Gen.OneOf(
            Gen.Elements(
                "Sarah Khan", "James Wilson", "Priya Patel", "Marcus Johnson",
                "Emma Roberts", "David Thompson", "Helen Clarke", "Richard Bennett",
                "Sophia Williams", "Afzal Ahmed", "Tom Harrison", "Li Wei",
                "Anna Schmidt", "John Smith", "Maria Garcia"),
            Gen.Choose(3, 30).SelectMany(len =>
                Gen.ArrayOf(len, Gen.Elements(
                    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
                    'k', 'l', 'm', 'n', 'o', 'p', ' '))
                .Select(chars => new string(chars).Trim())
                .Where(s => s.Length > 0)));

        return Arb.From(gen);
    }

    /// <summary>
    /// Generates non-empty role strings (1-100 characters) representing job roles.
    /// </summary>
    private static Arbitrary<string> NonEmptyRoleArb()
    {
        var gen = Gen.OneOf(
            Gen.Elements(
                "Software Engineer", "Senior Developer", "Project Manager",
                "Business Analyst", "QA Engineer", "DevOps Specialist",
                "Data Scientist", "Product Owner", "Scrum Master",
                "Technical Lead", "UX Designer", "Security Analyst",
                "Cloud Architect", "Full Stack Developer", "HR Coordinator"),
            Gen.Choose(3, 40).SelectMany(len =>
                Gen.ArrayOf(len, Gen.Elements(
                    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
                    'k', 'l', 'm', 'n', 'o', 'p', ' '))
                .Select(chars => new string(chars).Trim())
                .Where(s => s.Length > 0)));

        return Arb.From(gen);
    }

    /// <summary>
    /// Generates pairs of (employee name, manager name) as non-empty strings.
    /// </summary>
    private static Arbitrary<(string EmployeeName, string ManagerName)> EmployeeManagerPairArb()
    {
        var nameGen = Gen.OneOf(
            Gen.Elements(
                "Sarah Khan", "James Wilson", "Priya Patel", "Marcus Johnson",
                "Emma Roberts", "David Thompson", "Helen Clarke", "Richard Bennett",
                "Sophia Williams", "Afzal Ahmed", "Tom Harrison", "Li Wei",
                "Anna Schmidt", "John Smith", "Maria Garcia"));

        var gen = from employee in nameGen
                  from manager in nameGen
                  where employee != manager
                  select (employee, manager);

        return Arb.From(gen);
    }

    #endregion
}
