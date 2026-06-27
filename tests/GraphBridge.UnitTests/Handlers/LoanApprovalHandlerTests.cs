using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.LoanApprovals;
using GraphBridge.Application.LoanApprovals.Commands.CreateLoanApproval;
using GraphBridge.Application.LoanApprovals.Commands.GeneratePack;
using GraphBridge.Application.LoanApprovals.Commands.NotifyTeam;
using GraphBridge.Application.LoanApprovals.Commands.ScheduleFollowUp;
using GraphBridge.Application.LoanApprovals.Commands.SendCustomerEmail;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using Moq;

namespace GraphBridge.UnitTests.Handlers;

public class LoanApprovalHandlerTests
{
    private readonly Mock<ILoanApprovalStore> _loanStoreMock;
    private readonly Mock<IGraphMailService> _mailServiceMock;
    private readonly Mock<IGraphTeamsService> _teamsServiceMock;
    private readonly Mock<IGraphCalendarService> _calendarServiceMock;

    public LoanApprovalHandlerTests()
    {
        _loanStoreMock = new Mock<ILoanApprovalStore>();
        _mailServiceMock = new Mock<IGraphMailService>();
        _teamsServiceMock = new Mock<IGraphTeamsService>();
        _calendarServiceMock = new Mock<IGraphCalendarService>();
    }

    // --- CreateLoanApproval Handler ---

    [Fact]
    public async Task CreateLoanApproval_ShouldStoreAndReturnDto()
    {
        // Arrange
        _loanStoreMock.Setup(s => s.AddAsync(It.IsAny<LoanApproval>())).Returns(Task.CompletedTask);
        var handler = new CreateLoanApprovalCommandHandler(_loanStoreMock.Object);
        var command = new CreateLoanApprovalCommand
        {
            CustomerName = "John Smith",
            Amount = 250000m,
            PropertyReference = "PROP-001",
            Status = "Approved"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CustomerName.Should().Be("John Smith");
        result.Amount.Should().Be(250000m);
        result.PropertyReference.Should().Be("PROP-001");
        result.Status.Should().Be("Approved");
        result.PackGenerated.Should().BeFalse();
        _loanStoreMock.Verify(s => s.AddAsync(It.IsAny<LoanApproval>()), Times.Once);
    }

    // --- GeneratePack Handler ---

    [Fact]
    public async Task GeneratePack_WhenApproved_ShouldGeneratePackAndUpdateStore()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new LoanApproval
        {
            Id = loanId,
            CustomerName = "Jane Doe",
            Amount = 500000m,
            PropertyReference = "PROP-002",
            Status = "Approved",
            PackGenerated = false,
            AuditEntries = new List<LoanAuditEntry>()
        };
        _loanStoreMock.Setup(s => s.GetByIdAsync(loanId)).ReturnsAsync(loan);
        _loanStoreMock.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>())).Returns(Task.CompletedTask);

        var handler = new GeneratePackCommandHandler(_loanStoreMock.Object);
        var command = new GeneratePackCommand { LoanId = loanId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerEmail.Should().NotBeNull();
        result.CustomerEmail.Subject.Should().NotBeNullOrEmpty();
        result.CustomerEmail.Body.Should().Contain("Jane Doe");
        result.InternalNotificationContent.Should().NotBeNullOrEmpty();
        result.DocumentChecklist.Should().NotBeEmpty();
        _loanStoreMock.Verify(s => s.UpdateAsync(It.Is<LoanApproval>(l => l.PackGenerated == true)), Times.Once);
    }

    [Fact]
    public async Task GeneratePack_WhenNotApproved_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new LoanApproval
        {
            Id = loanId,
            CustomerName = "Bob Jones",
            Amount = 300000m,
            PropertyReference = "PROP-003",
            Status = "Pending",
            PackGenerated = false,
            AuditEntries = new List<LoanAuditEntry>()
        };
        _loanStoreMock.Setup(s => s.GetByIdAsync(loanId)).ReturnsAsync(loan);

        var handler = new GeneratePackCommandHandler(_loanStoreMock.Object);
        var command = new GeneratePackCommand { LoanId = loanId };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*approved*");
    }

    // --- SendCustomerEmail Handler ---

    [Fact]
    public async Task SendCustomerEmail_WhenPackGenerated_ShouldSendEmailAndCreateAuditEntry()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new LoanApproval
        {
            Id = loanId,
            CustomerName = "Alice Brown",
            Amount = 150000m,
            PropertyReference = "PROP-004",
            Status = "Approved",
            PackGenerated = true,
            AuditEntries = new List<LoanAuditEntry>()
        };
        _loanStoreMock.Setup(s => s.GetByIdAsync(loanId)).ReturnsAsync(loan);
        _loanStoreMock.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>())).Returns(Task.CompletedTask);
        _mailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SendCustomerEmailCommandHandler(_loanStoreMock.Object, _mailServiceMock.Object);
        var command = new SendCustomerEmailCommand { LoanId = loanId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        loan.AuditEntries.Should().HaveCount(1);
        loan.AuditEntries[0].ActionType.Should().Be("CustomerEmailSent");
        loan.AuditEntries[0].Status.Should().Be("Completed");
        _loanStoreMock.Verify(s => s.UpdateAsync(loan), Times.Once);
    }

    [Fact]
    public async Task SendCustomerEmail_WhenPackNotGenerated_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new LoanApproval
        {
            Id = loanId,
            CustomerName = "Charlie Davis",
            Amount = 200000m,
            PropertyReference = "PROP-005",
            Status = "Approved",
            PackGenerated = false,
            AuditEntries = new List<LoanAuditEntry>()
        };
        _loanStoreMock.Setup(s => s.GetByIdAsync(loanId)).ReturnsAsync(loan);

        var handler = new SendCustomerEmailCommandHandler(_loanStoreMock.Object, _mailServiceMock.Object);
        var command = new SendCustomerEmailCommand { LoanId = loanId };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*pack*generated*");
        _mailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- NotifyTeam Handler ---

    [Fact]
    public async Task NotifyTeam_ShouldSendTeamsNotificationAndCreateAuditEntry()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new LoanApproval
        {
            Id = loanId,
            CustomerName = "Diana Evans",
            Amount = 400000m,
            PropertyReference = "PROP-006",
            Status = "Approved",
            PackGenerated = true,
            AuditEntries = new List<LoanAuditEntry>()
        };
        _loanStoreMock.Setup(s => s.GetByIdAsync(loanId)).ReturnsAsync(loan);
        _loanStoreMock.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>())).Returns(Task.CompletedTask);
        _teamsServiceMock.Setup(t => t.SendChannelNotificationAsync(It.IsAny<SendChannelNotificationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new NotifyTeamCommandHandler(_loanStoreMock.Object, _teamsServiceMock.Object);
        var command = new NotifyTeamCommand { LoanId = loanId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _teamsServiceMock.Verify(t => t.SendChannelNotificationAsync(It.IsAny<SendChannelNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        loan.AuditEntries.Should().HaveCount(1);
        loan.AuditEntries[0].ActionType.Should().Be("TeamNotified");
        loan.AuditEntries[0].Status.Should().Be("Completed");
        _loanStoreMock.Verify(s => s.UpdateAsync(loan), Times.Once);
    }

    // --- ScheduleFollowUp Handler ---

    [Fact]
    public async Task ScheduleFollowUp_ShouldCreateCalendarEventAndCreateAuditEntry()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var loan = new LoanApproval
        {
            Id = loanId,
            CustomerName = "Edward Foster",
            Amount = 600000m,
            PropertyReference = "PROP-007",
            Status = "Approved",
            PackGenerated = true,
            AuditEntries = new List<LoanAuditEntry>()
        };
        _loanStoreMock.Setup(s => s.GetByIdAsync(loanId)).ReturnsAsync(loan);
        _loanStoreMock.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>())).Returns(Task.CompletedTask);
        _calendarServiceMock.Setup(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CalendarEventDto { Subject = "Follow-up", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) });

        var handler = new ScheduleFollowUpCommandHandler(_loanStoreMock.Object, _calendarServiceMock.Object);
        var command = new ScheduleFollowUpCommand { LoanId = loanId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _calendarServiceMock.Verify(c => c.CreateEventAsync(It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        loan.AuditEntries.Should().HaveCount(1);
        loan.AuditEntries[0].ActionType.Should().Be("FollowUpScheduled");
        loan.AuditEntries[0].Status.Should().Be("Completed");
        _loanStoreMock.Verify(s => s.UpdateAsync(loan), Times.Once);
    }
}
