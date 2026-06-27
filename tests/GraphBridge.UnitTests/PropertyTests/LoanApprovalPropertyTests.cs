using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Application.LoanApprovals;
using GraphBridge.Application.LoanApprovals.Commands.GeneratePack;
using GraphBridge.Application.LoanApprovals.Commands.NotifyTeam;
using GraphBridge.Application.LoanApprovals.Commands.ScheduleFollowUp;
using GraphBridge.Application.LoanApprovals.Commands.SendCustomerEmail;
using GraphBridge.Application.LoanApprovals.Queries.GetAudit;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using Moq;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property tests for the Loan Approval Communication Hub Module.
/// Tests properties 15, 16, 17, and 18 from the design document.
/// </summary>
public class LoanApprovalPropertyTests
{
    #region Property 15: Loan Operation Ordering Enforcement

    /// <summary>
    /// Property 15: Loan Operation Ordering Enforcement
    /// For any loan NOT in "Approved" status, generate-pack SHALL fail with a BusinessRuleException.
    /// The system SHALL never allow pack generation when the loan status is anything other than "Approved".
    ///
    /// **Validates: Requirements 7.3, 7.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property GeneratePack_Fails_ForNonApprovedStatus()
    {
        return Prop.ForAll(
            Arb.From(
                from status in Gen.Elements("Pending", "Rejected", "Under Review", "Submitted", "Cancelled", "Draft", "")
                from customerName in Gen.Elements("John Smith", "Sarah Khan", "James Wilson", "Alice Brown")
                from amount in Gen.Choose(100, 999999).Select(a => (decimal)a)
                from propRef in Gen.Elements("PROP-001", "PROP-999", "REF-2024-ABC")
                select new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = customerName,
                    Amount = amount,
                    PropertyReference = propRef,
                    Status = status,
                    PackGenerated = false,
                    AuditEntries = new List<LoanAuditEntry>()
                }),
            loan =>
            {
                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);

                var handler = new GeneratePackCommandHandler(mockStore.Object);
                var command = new GeneratePackCommand { LoanId = loan.Id };

                var act = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                act.Should().Throw<BusinessRuleException>()
                    .WithMessage("*approved loans*");
            });
    }

    /// <summary>
    /// Property 15: Loan Operation Ordering Enforcement
    /// For any loan without pack generated (PackGenerated = false), send-customer-email
    /// SHALL fail with a BusinessRuleException. The system SHALL never allow sending email
    /// before the communication pack is generated.
    ///
    /// **Validates: Requirements 7.3, 7.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SendCustomerEmail_Fails_WhenPackNotGenerated()
    {
        return Prop.ForAll(
            Arb.From(
                from customerName in Gen.Elements("John Smith", "Sarah Khan", "James Wilson", "Alice Brown", "Robert Taylor")
                from amount in Gen.Choose(100, 999999).Select(a => (decimal)a)
                from propRef in Gen.Elements("PROP-001", "PROP-999", "REF-2024-ABC", "LAND-567")
                from status in Gen.Elements("Approved", "Pending", "Rejected")
                select new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = customerName,
                    Amount = amount,
                    PropertyReference = propRef,
                    Status = status,
                    PackGenerated = false,
                    AuditEntries = new List<LoanAuditEntry>()
                }),
            loan =>
            {
                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);

                var mockMailService = new Mock<IGraphMailService>();

                var handler = new SendCustomerEmailCommandHandler(mockStore.Object, mockMailService.Object);
                var command = new SendCustomerEmailCommand { LoanId = loan.Id };

                var act = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                act.Should().Throw<BusinessRuleException>()
                    .WithMessage("*pack*generated*");

                // Verify mail service was never called
                mockMailService.Verify(
                    m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()),
                    Times.Never);
            });
    }

    #endregion

    #region Property 16: Communication Pack Completeness

    /// <summary>
    /// Property 16: Communication Pack Completeness
    /// For any approved loan, the generate-pack action SHALL produce a communication pack containing:
    /// customer email content with non-empty subject and body, internal notification content with
    /// non-empty summary text, and a document checklist with at least one item.
    ///
    /// **Validates: Requirements 7.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property GeneratePack_ProducesCompletePackForApprovedLoan()
    {
        return Prop.ForAll(
            Arb.From(
                from customerName in Gen.Elements(
                    "John Smith", "Sarah Khan", "James Wilson", "Alice Brown",
                    "Robert Taylor", "Emily Davis", "Michael Johnson", "Hannah Clarke")
                from amount in Gen.Choose(1, 99999999).Select(a => (decimal)a / 100m)
                from propRef in Gen.Elements(
                    "PROP-001", "PROP-999", "REF-2024-ABC", "LAND-567",
                    "HOUSE-42", "FLAT-123", "BUILDING-789")
                select new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = customerName,
                    Amount = amount,
                    PropertyReference = propRef,
                    Status = "Approved",
                    PackGenerated = false,
                    AuditEntries = new List<LoanAuditEntry>()
                }),
            loan =>
            {
                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>()))
                    .Returns(Task.CompletedTask);

                var handler = new GeneratePackCommandHandler(mockStore.Object);
                var command = new GeneratePackCommand { LoanId = loan.Id };

                var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Customer email must have non-empty subject and body
                result.CustomerEmail.Should().NotBeNull("Communication pack must contain customer email");
                result.CustomerEmail.Subject.Should().NotBeNullOrEmpty(
                    "Customer email subject must be non-empty");
                result.CustomerEmail.Body.Should().NotBeNullOrEmpty(
                    "Customer email body must be non-empty");

                // Internal notification must be non-empty
                result.InternalNotificationContent.Should().NotBeNullOrEmpty(
                    "Internal notification content must be non-empty");

                // Document checklist must have at least one item
                result.DocumentChecklist.Should().NotBeNull("Document checklist must not be null");
                result.DocumentChecklist.Should().HaveCountGreaterOrEqualTo(1,
                    "Document checklist must have at least one item");
                result.DocumentChecklist.Should().AllSatisfy(item =>
                    item.Should().NotBeNullOrEmpty("Each checklist item must be non-empty"));
            });
    }

    #endregion

    #region Property 17: Audit Trail Chronological Order and Limit

    /// <summary>
    /// Property 17: Audit Trail Chronological Order and Limit
    /// For any loan approval with N audit entries (where N >= 0), requesting the audit trail
    /// SHALL return entries in chronological order (timestamps non-decreasing), and the result
    /// SHALL contain at most 100 entries. Each entry SHALL have a non-empty actionType, a valid
    /// timestamp, and a non-empty status.
    ///
    /// **Validates: Requirements 7.8**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AuditTrail_IsChronological_AndLimitedTo100()
    {
        return Prop.ForAll(
            Arb.From(
                from entryCount in Gen.Choose(0, 200)
                select entryCount),
            entryCount =>
            {
                var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var random = new System.Random(entryCount);
                var actionTypes = new[] { "CustomerEmailSent", "TeamNotified", "FollowUpScheduled", "PackGenerated" };

                // Generate audit entries with random (non-sequential) timestamps
                var entries = Enumerable.Range(0, entryCount)
                    .Select(i => new LoanAuditEntry
                    {
                        ActionType = actionTypes[random.Next(actionTypes.Length)],
                        Timestamp = baseTime.AddMinutes(random.Next(0, 10000)),
                        Status = "Completed"
                    })
                    .ToList();

                var loan = new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Test Customer",
                    Amount = 100000m,
                    PropertyReference = "PROP-001",
                    Status = "Approved",
                    PackGenerated = true,
                    AuditEntries = entries
                };

                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);

                var handler = new GetAuditQueryHandler(mockStore.Object);
                var query = new GetAuditQuery { LoanId = loan.Id };

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                // Result must be limited to 100 entries
                result.Count.Should().BeLessOrEqualTo(100,
                    "Audit trail must be limited to at most 100 entries");

                // Result must be in chronological order (non-decreasing timestamps)
                for (int i = 1; i < result.Count; i++)
                {
                    result[i].Timestamp.Should().BeOnOrAfter(result[i - 1].Timestamp,
                        $"Entry at index {i} must have a timestamp >= the previous entry (chronological order)");
                }

                // Each entry must have non-empty actionType and status
                foreach (var entry in result)
                {
                    entry.ActionType.Should().NotBeNullOrEmpty(
                        "Each audit entry must have a non-empty ActionType");
                    entry.Status.Should().NotBeNullOrEmpty(
                        "Each audit entry must have a non-empty Status");
                }
            });
    }

    #endregion

    #region Property 18: Audit Entry Creation on Communication Actions

    /// <summary>
    /// Property 18: Audit Entry Creation on Communication Actions
    /// When send-customer-email succeeds, the system SHALL create an audit trail entry with
    /// ActionType "CustomerEmailSent" and a valid timestamp, and the total audit entry count
    /// SHALL increase by exactly one.
    ///
    /// **Validates: Requirements 7.4, 7.6, 7.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SendCustomerEmail_CreatesAuditEntry_WithCorrectActionType()
    {
        return Prop.ForAll(
            Arb.From(
                from customerName in Gen.Elements("John Smith", "Sarah Khan", "James Wilson", "Alice Brown")
                from amount in Gen.Choose(100, 999999).Select(a => (decimal)a)
                from propRef in Gen.Elements("PROP-001", "PROP-999", "REF-2024-ABC")
                from existingEntryCount in Gen.Choose(0, 10)
                select (customerName, amount, propRef, existingEntryCount)),
            data =>
            {
                var existingEntries = Enumerable.Range(0, data.existingEntryCount)
                    .Select(i => new LoanAuditEntry
                    {
                        ActionType = "PreviousAction",
                        Timestamp = DateTime.UtcNow.AddDays(-i - 1),
                        Status = "Completed"
                    })
                    .ToList();

                var loan = new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = data.customerName,
                    Amount = data.amount,
                    PropertyReference = data.propRef,
                    Status = "Approved",
                    PackGenerated = true,
                    AuditEntries = existingEntries
                };

                var initialCount = loan.AuditEntries.Count;
                var beforeExecution = DateTime.UtcNow;

                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>()))
                    .Returns(Task.CompletedTask);

                var mockMailService = new Mock<IGraphMailService>();
                mockMailService.Setup(m => m.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var handler = new SendCustomerEmailCommandHandler(mockStore.Object, mockMailService.Object);
                var command = new SendCustomerEmailCommand { LoanId = loan.Id };

                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Audit entry count must increase by exactly one
                loan.AuditEntries.Count.Should().Be(initialCount + 1,
                    "Communication action must add exactly one audit entry");

                // The new entry must have the correct ActionType
                var newEntry = loan.AuditEntries.Last();
                newEntry.ActionType.Should().Be("CustomerEmailSent",
                    "send-customer-email action must create an entry with ActionType 'CustomerEmailSent'");
                newEntry.Status.Should().NotBeNullOrEmpty("Audit entry status must be non-empty");
                newEntry.Timestamp.Should().BeOnOrAfter(beforeExecution,
                    "Audit entry timestamp must be at or after execution time");
            });
    }

    /// <summary>
    /// Property 18: Audit Entry Creation on Communication Actions
    /// When notify-team succeeds, the system SHALL create an audit trail entry with
    /// ActionType "TeamNotified" and a valid timestamp, and the total audit entry count
    /// SHALL increase by exactly one.
    ///
    /// **Validates: Requirements 7.4, 7.6, 7.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NotifyTeam_CreatesAuditEntry_WithCorrectActionType()
    {
        return Prop.ForAll(
            Arb.From(
                from customerName in Gen.Elements("John Smith", "Sarah Khan", "James Wilson", "Alice Brown")
                from amount in Gen.Choose(100, 999999).Select(a => (decimal)a)
                from propRef in Gen.Elements("PROP-001", "PROP-999", "REF-2024-ABC")
                from existingEntryCount in Gen.Choose(0, 10)
                select (customerName, amount, propRef, existingEntryCount)),
            data =>
            {
                var existingEntries = Enumerable.Range(0, data.existingEntryCount)
                    .Select(i => new LoanAuditEntry
                    {
                        ActionType = "PreviousAction",
                        Timestamp = DateTime.UtcNow.AddDays(-i - 1),
                        Status = "Completed"
                    })
                    .ToList();

                var loan = new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = data.customerName,
                    Amount = data.amount,
                    PropertyReference = data.propRef,
                    Status = "Approved",
                    PackGenerated = true,
                    AuditEntries = existingEntries
                };

                var initialCount = loan.AuditEntries.Count;
                var beforeExecution = DateTime.UtcNow;

                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>()))
                    .Returns(Task.CompletedTask);

                var mockTeamsService = new Mock<IGraphTeamsService>();
                mockTeamsService.Setup(t => t.SendChannelNotificationAsync(
                        It.IsAny<SendChannelNotificationRequest>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var handler = new NotifyTeamCommandHandler(mockStore.Object, mockTeamsService.Object);
                var command = new NotifyTeamCommand { LoanId = loan.Id };

                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Audit entry count must increase by exactly one
                loan.AuditEntries.Count.Should().Be(initialCount + 1,
                    "Communication action must add exactly one audit entry");

                // The new entry must have the correct ActionType
                var newEntry = loan.AuditEntries.Last();
                newEntry.ActionType.Should().Be("TeamNotified",
                    "notify-team action must create an entry with ActionType 'TeamNotified'");
                newEntry.Status.Should().NotBeNullOrEmpty("Audit entry status must be non-empty");
                newEntry.Timestamp.Should().BeOnOrAfter(beforeExecution,
                    "Audit entry timestamp must be at or after execution time");
            });
    }

    /// <summary>
    /// Property 18: Audit Entry Creation on Communication Actions
    /// When schedule-follow-up succeeds, the system SHALL create an audit trail entry with
    /// ActionType "FollowUpScheduled" and a valid timestamp, and the total audit entry count
    /// SHALL increase by exactly one.
    ///
    /// **Validates: Requirements 7.4, 7.6, 7.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ScheduleFollowUp_CreatesAuditEntry_WithCorrectActionType()
    {
        return Prop.ForAll(
            Arb.From(
                from customerName in Gen.Elements("John Smith", "Sarah Khan", "James Wilson", "Alice Brown")
                from amount in Gen.Choose(100, 999999).Select(a => (decimal)a)
                from propRef in Gen.Elements("PROP-001", "PROP-999", "REF-2024-ABC")
                from existingEntryCount in Gen.Choose(0, 10)
                select (customerName, amount, propRef, existingEntryCount)),
            data =>
            {
                var existingEntries = Enumerable.Range(0, data.existingEntryCount)
                    .Select(i => new LoanAuditEntry
                    {
                        ActionType = "PreviousAction",
                        Timestamp = DateTime.UtcNow.AddDays(-i - 1),
                        Status = "Completed"
                    })
                    .ToList();

                var loan = new LoanApproval
                {
                    Id = Guid.NewGuid(),
                    CustomerName = data.customerName,
                    Amount = data.amount,
                    PropertyReference = data.propRef,
                    Status = "Approved",
                    PackGenerated = true,
                    AuditEntries = existingEntries
                };

                var initialCount = loan.AuditEntries.Count;
                var beforeExecution = DateTime.UtcNow;

                var mockStore = new Mock<ILoanApprovalStore>();
                mockStore.Setup(s => s.GetByIdAsync(loan.Id))
                    .ReturnsAsync(loan);
                mockStore.Setup(s => s.UpdateAsync(It.IsAny<LoanApproval>()))
                    .Returns(Task.CompletedTask);

                var mockCalendarService = new Mock<IGraphCalendarService>();
                mockCalendarService.Setup(c => c.CreateEventAsync(
                        It.IsAny<CreateCalendarEventRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CalendarEventDto
                    {
                        Subject = "Follow-up",
                        Start = DateTime.UtcNow.AddDays(7),
                        End = DateTime.UtcNow.AddDays(7).AddHours(1),
                        Attendees = new List<string> { data.customerName }
                    });

                var handler = new ScheduleFollowUpCommandHandler(mockStore.Object, mockCalendarService.Object);
                var command = new ScheduleFollowUpCommand { LoanId = loan.Id };

                handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

                // Audit entry count must increase by exactly one
                loan.AuditEntries.Count.Should().Be(initialCount + 1,
                    "Communication action must add exactly one audit entry");

                // The new entry must have the correct ActionType
                var newEntry = loan.AuditEntries.Last();
                newEntry.ActionType.Should().Be("FollowUpScheduled",
                    "schedule-follow-up action must create an entry with ActionType 'FollowUpScheduled'");
                newEntry.Status.Should().NotBeNullOrEmpty("Audit entry status must be non-empty");
                newEntry.Timestamp.Should().BeOnOrAfter(beforeExecution,
                    "Audit entry timestamp must be at or after execution time");
            });
    }

    #endregion
}
