using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.Onboarding;
using GraphBridge.Application.Onboarding.Commands.AssignGroups;
using GraphBridge.Application.Onboarding.Commands.CreateEmployee;
using GraphBridge.Application.Onboarding.Commands.ScheduleInduction;
using GraphBridge.Application.Onboarding.Commands.SendWelcomeEmail;
using GraphBridge.Application.Onboarding.Queries.GetEmployeeById;
using GraphBridge.Application.Onboarding.Queries.GetEmployeeStatus;
using GraphBridge.Application.Onboarding.Queries.GetOnboardingOverview;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using MediatR;
using Moq;

namespace GraphBridge.UnitTests.Handlers;

public class OnboardingHandlerTests
{
    private readonly Mock<IEmployeeStore> _employeeStoreMock = new();

    #region CreateEmployee Handler

    [Fact]
    public async Task CreateEmployeeHandler_ValidCommand_StoresEmployeeAndReturnsDto()
    {
        // Arrange
        var handler = new CreateEmployeeCommandHandler(_employeeStoreMock.Object);
        var command = new CreateEmployeeCommand
        {
            Name = "Sarah Khan",
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Afzal Ahmed",
            Email = "sarah.khan@company.com"
        };

        _employeeStoreMock
            .Setup(s => s.AddAsync(It.IsAny<EmployeeOnboarding>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Sarah Khan");
        result.Role.Should().Be("Software Engineer");
        result.Department.Should().Be("Engineering");
        result.ManagerName.Should().Be("Afzal Ahmed");
        result.Email.Should().Be("sarah.khan@company.com");
        result.Status.ProfileCreated.Should().BeTrue();
        result.Status.GroupsAssigned.Should().BeFalse();
        result.Status.WelcomeEmailSent.Should().BeFalse();
        result.Status.InductionScheduled.Should().BeFalse();

        _employeeStoreMock.Verify(s => s.AddAsync(It.Is<EmployeeOnboarding>(e =>
            e.Name == "Sarah Khan" &&
            e.ProfileCreated == true)), Times.Once);
    }

    #endregion

    #region AssignGroups Handler

    [Fact]
    public async Task AssignGroupsHandler_ExistingEmployee_CallsGraphGroupServiceAndUpdatesFlag()
    {
        // Arrange
        var graphGroupServiceMock = new Mock<IGraphGroupService>();
        var handler = new AssignGroupsCommandHandler(_employeeStoreMock.Object, graphGroupServiceMock.Object);

        var employeeId = Guid.NewGuid();
        var employee = new EmployeeOnboarding
        {
            Id = employeeId,
            Name = "Sarah Khan",
            Department = "Engineering",
            GroupsAssigned = false
        };

        _employeeStoreMock.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeStoreMock.Setup(s => s.UpdateAsync(It.IsAny<EmployeeOnboarding>())).Returns(Task.CompletedTask);

        var groups = new List<GroupDto>
        {
            new() { Id = "group-1", DisplayName = "Engineering Team", Description = "Engineering group" }
        };
        graphGroupServiceMock
            .Setup(g => g.GetGroupsForDepartmentAsync("Engineering", It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);
        graphGroupServiceMock
            .Setup(g => g.AssignUserToGroupsAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(new AssignGroupsCommand { EmployeeId = employeeId }, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        graphGroupServiceMock.Verify(g => g.GetGroupsForDepartmentAsync("Engineering", It.IsAny<CancellationToken>()), Times.Once);
        graphGroupServiceMock.Verify(g => g.AssignUserToGroupsAsync(
            employeeId.ToString(),
            It.Is<IReadOnlyList<string>>(ids => ids.Contains("group-1")),
            It.IsAny<CancellationToken>()), Times.Once);
        _employeeStoreMock.Verify(s => s.UpdateAsync(It.Is<EmployeeOnboarding>(e => e.GroupsAssigned == true)), Times.Once);
    }

    [Fact]
    public async Task AssignGroupsHandler_NonExistentEmployee_ThrowsNotFoundException()
    {
        // Arrange
        var graphGroupServiceMock = new Mock<IGraphGroupService>();
        var handler = new AssignGroupsCommandHandler(_employeeStoreMock.Object, graphGroupServiceMock.Object);
        var nonExistentId = Guid.NewGuid();

        _employeeStoreMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((EmployeeOnboarding?)null);

        // Act
        var act = () => handler.Handle(new AssignGroupsCommand { EmployeeId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region SendWelcomeEmail Handler

    [Fact]
    public async Task SendWelcomeEmailHandler_ExistingEmployee_CallsMailServiceWithCorrectContent()
    {
        // Arrange
        var mailServiceMock = new Mock<IGraphMailService>();
        var handler = new SendWelcomeEmailCommandHandler(_employeeStoreMock.Object, mailServiceMock.Object);

        var employeeId = Guid.NewGuid();
        var employee = new EmployeeOnboarding
        {
            Id = employeeId,
            Name = "Sarah Khan",
            Role = "Software Engineer",
            Email = "sarah.khan@company.com",
            WelcomeEmailSent = false
        };

        _employeeStoreMock.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeStoreMock.Setup(s => s.UpdateAsync(It.IsAny<EmployeeOnboarding>())).Returns(Task.CompletedTask);
        mailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(new SendWelcomeEmailCommand { EmployeeId = employeeId }, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        mailServiceMock.Verify(m => m.SendEmailAsync(
            It.Is<SendEmailRequest>(r =>
                r.To == "sarah.khan@company.com" &&
                r.Body.Contains("Sarah Khan") &&
                r.Body.Contains("Software Engineer")),
            It.IsAny<CancellationToken>()), Times.Once);
        _employeeStoreMock.Verify(s => s.UpdateAsync(It.Is<EmployeeOnboarding>(e => e.WelcomeEmailSent == true)), Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailHandler_NonExistentEmployee_ThrowsNotFoundException()
    {
        // Arrange
        var mailServiceMock = new Mock<IGraphMailService>();
        var handler = new SendWelcomeEmailCommandHandler(_employeeStoreMock.Object, mailServiceMock.Object);
        var nonExistentId = Guid.NewGuid();

        _employeeStoreMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((EmployeeOnboarding?)null);

        // Act
        var act = () => handler.Handle(new SendWelcomeEmailCommand { EmployeeId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ScheduleInduction Handler

    [Fact]
    public async Task ScheduleInductionHandler_ExistingEmployee_CallsCalendarServiceWith60MinEvent()
    {
        // Arrange
        var calendarServiceMock = new Mock<IGraphCalendarService>();
        var handler = new ScheduleInductionCommandHandler(_employeeStoreMock.Object, calendarServiceMock.Object);

        var employeeId = Guid.NewGuid();
        var employee = new EmployeeOnboarding
        {
            Id = employeeId,
            Name = "Sarah Khan",
            Email = "sarah.khan@company.com",
            ManagerName = "Afzal Ahmed",
            InductionScheduled = false
        };

        _employeeStoreMock.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeStoreMock.Setup(s => s.UpdateAsync(It.IsAny<EmployeeOnboarding>())).Returns(Task.CompletedTask);
        calendarServiceMock
            .Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalendarEventDto { Subject = "Induction Meeting" });

        // Act
        var result = await handler.Handle(new ScheduleInductionCommand { EmployeeId = employeeId }, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        calendarServiceMock.Verify(c => c.CreateEventAsync(
            It.Is<CreateCalendarEventRequest>(r =>
                r.Subject == "Induction Meeting" &&
                (r.End - r.Start).TotalMinutes == 60 &&
                r.Attendees.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
        _employeeStoreMock.Verify(s => s.UpdateAsync(It.Is<EmployeeOnboarding>(e => e.InductionScheduled == true)), Times.Once);
    }

    [Fact]
    public async Task ScheduleInductionHandler_NonExistentEmployee_ThrowsNotFoundException()
    {
        // Arrange
        var calendarServiceMock = new Mock<IGraphCalendarService>();
        var handler = new ScheduleInductionCommandHandler(_employeeStoreMock.Object, calendarServiceMock.Object);
        var nonExistentId = Guid.NewGuid();

        _employeeStoreMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((EmployeeOnboarding?)null);

        // Act
        var act = () => handler.Handle(new ScheduleInductionCommand { EmployeeId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetEmployeeById Handler

    [Fact]
    public async Task GetEmployeeByIdHandler_ExistingEmployee_ReturnsCorrectDto()
    {
        // Arrange
        var handler = new GetEmployeeByIdQueryHandler(_employeeStoreMock.Object);
        var employeeId = Guid.NewGuid();
        var employee = new EmployeeOnboarding
        {
            Id = employeeId,
            Name = "Sarah Khan",
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Afzal Ahmed",
            Email = "sarah.khan@company.com",
            ProfileCreated = true,
            GroupsAssigned = true,
            WelcomeEmailSent = false,
            InductionScheduled = false
        };

        _employeeStoreMock.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);

        // Act
        var result = await handler.Handle(new GetEmployeeByIdQuery { EmployeeId = employeeId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(employeeId);
        result.Name.Should().Be("Sarah Khan");
        result.Status.ProfileCreated.Should().BeTrue();
        result.Status.GroupsAssigned.Should().BeTrue();
    }

    [Fact]
    public async Task GetEmployeeByIdHandler_NonExistentEmployee_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetEmployeeByIdQueryHandler(_employeeStoreMock.Object);
        var nonExistentId = Guid.NewGuid();

        _employeeStoreMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((EmployeeOnboarding?)null);

        // Act
        var act = () => handler.Handle(new GetEmployeeByIdQuery { EmployeeId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetEmployeeStatus Handler

    [Fact]
    public async Task GetEmployeeStatusHandler_ExistingEmployee_ReturnsStatusDto()
    {
        // Arrange
        var handler = new GetEmployeeStatusQueryHandler(_employeeStoreMock.Object);
        var employeeId = Guid.NewGuid();
        var employee = new EmployeeOnboarding
        {
            Id = employeeId,
            ProfileCreated = true,
            GroupsAssigned = true,
            WelcomeEmailSent = true,
            InductionScheduled = false
        };

        _employeeStoreMock.Setup(s => s.GetByIdAsync(employeeId)).ReturnsAsync(employee);

        // Act
        var result = await handler.Handle(new GetEmployeeStatusQuery { EmployeeId = employeeId }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProfileCreated.Should().BeTrue();
        result.GroupsAssigned.Should().BeTrue();
        result.WelcomeEmailSent.Should().BeTrue();
        result.InductionScheduled.Should().BeFalse();
    }

    [Fact]
    public async Task GetEmployeeStatusHandler_NonExistentEmployee_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetEmployeeStatusQueryHandler(_employeeStoreMock.Object);
        var nonExistentId = Guid.NewGuid();

        _employeeStoreMock.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((EmployeeOnboarding?)null);

        // Act
        var act = () => handler.Handle(new GetEmployeeStatusQuery { EmployeeId = nonExistentId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetOnboardingOverview Handler

    [Fact]
    public async Task GetOnboardingOverviewHandler_ReturnsAllEmployeesAsDtos()
    {
        // Arrange
        var handler = new GetOnboardingOverviewQueryHandler(_employeeStoreMock.Object);
        var employees = new List<EmployeeOnboarding>
        {
            new() { Id = Guid.NewGuid(), Name = "Sarah Khan", Role = "Engineer", Department = "Eng", ProfileCreated = true },
            new() { Id = Guid.NewGuid(), Name = "John Smith", Role = "Designer", Department = "Design", ProfileCreated = true }
        };

        _employeeStoreMock.Setup(s => s.GetAllAsync()).ReturnsAsync(employees);

        // Act
        var result = await handler.Handle(new GetOnboardingOverviewQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Sarah Khan");
        result[1].Name.Should().Be("John Smith");
    }

    #endregion
}
