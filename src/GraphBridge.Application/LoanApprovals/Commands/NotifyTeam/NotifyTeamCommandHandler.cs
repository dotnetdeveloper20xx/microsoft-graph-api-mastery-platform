using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.NotifyTeam;

/// <summary>
/// Handles sending a Teams notification about the loan approval to the internal finance channel.
/// Creates an audit trail entry upon successful notification.
/// </summary>
public class NotifyTeamCommandHandler : IRequestHandler<NotifyTeamCommand, Unit>
{
    private readonly ILoanApprovalStore _loanStore;
    private readonly IGraphTeamsService _teamsService;

    public NotifyTeamCommandHandler(ILoanApprovalStore loanStore, IGraphTeamsService teamsService)
    {
        _loanStore = loanStore;
        _teamsService = teamsService;
    }

    public async Task<Unit> Handle(NotifyTeamCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanStore.GetByIdAsync(request.LoanId)
            ?? throw new NotFoundException("LoanApproval", request.LoanId);

        await _teamsService.SendChannelNotificationAsync(new SendChannelNotificationRequest
        {
            TeamId = "finance-team",
            ChannelId = "loan-approvals",
            Message = $"Loan approved for {loan.CustomerName} - Amount: £{loan.Amount:N2}, Property: {loan.PropertyReference}"
        }, cancellationToken);

        var auditEntry = new LoanAuditEntry
        {
            ActionType = "TeamNotified",
            Timestamp = DateTime.UtcNow,
            Status = "Completed"
        };

        loan.AuditEntries.Add(auditEntry);
        await _loanStore.UpdateAsync(loan);

        return Unit.Value;
    }
}
