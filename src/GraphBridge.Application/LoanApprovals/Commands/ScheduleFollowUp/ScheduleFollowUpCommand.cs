using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.ScheduleFollowUp;

/// <summary>
/// Command to schedule a follow-up calendar event for the customer.
/// </summary>
public class ScheduleFollowUpCommand : IRequest<Unit>
{
    public Guid LoanId { get; set; }
}
