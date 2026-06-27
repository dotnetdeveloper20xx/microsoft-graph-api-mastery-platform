using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.NotifyTeam;

/// <summary>
/// Command to send a Teams notification to the internal finance channel about the loan approval.
/// </summary>
public class NotifyTeamCommand : IRequest<Unit>
{
    public Guid LoanId { get; set; }
}
