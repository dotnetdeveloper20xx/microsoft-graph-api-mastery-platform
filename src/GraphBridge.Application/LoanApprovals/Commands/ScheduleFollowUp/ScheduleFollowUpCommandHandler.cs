using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.ScheduleFollowUp;

/// <summary>
/// Handles scheduling a follow-up calendar event for the customer.
/// Creates a calendar event 7 days from now with 60-minute duration,
/// and records an audit trail entry.
/// </summary>
public class ScheduleFollowUpCommandHandler : IRequestHandler<ScheduleFollowUpCommand, Unit>
{
    private readonly ILoanApprovalStore _loanStore;
    private readonly IGraphCalendarService _calendarService;

    public ScheduleFollowUpCommandHandler(ILoanApprovalStore loanStore, IGraphCalendarService calendarService)
    {
        _loanStore = loanStore;
        _calendarService = calendarService;
    }

    public async Task<Unit> Handle(ScheduleFollowUpCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanStore.GetByIdAsync(request.LoanId)
            ?? throw new NotFoundException("LoanApproval", request.LoanId);

        var startTime = DateTime.UtcNow.AddDays(7);

        await _calendarService.CreateEventAsync(new CreateCalendarEventRequest
        {
            Subject = $"Follow-up: {loan.CustomerName}",
            Start = startTime,
            End = startTime.AddMinutes(60),
            Attendees = new List<string> { loan.CustomerName }
        }, cancellationToken);

        var auditEntry = new LoanAuditEntry
        {
            ActionType = "FollowUpScheduled",
            Timestamp = DateTime.UtcNow,
            Status = "Completed"
        };

        loan.AuditEntries.Add(auditEntry);
        await _loanStore.UpdateAsync(loan);

        return Unit.Value;
    }
}
