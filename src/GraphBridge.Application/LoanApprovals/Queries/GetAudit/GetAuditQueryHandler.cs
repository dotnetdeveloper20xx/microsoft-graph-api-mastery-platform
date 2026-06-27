using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Queries.GetAudit;

/// <summary>
/// Handles retrieving the audit trail for a specific loan approval.
/// Returns entries ordered chronologically (by Timestamp ascending), limited to 100 entries.
/// </summary>
public class GetAuditQueryHandler : IRequestHandler<GetAuditQuery, IReadOnlyList<AuditEntryDto>>
{
    private readonly ILoanApprovalStore _loanStore;

    public GetAuditQueryHandler(ILoanApprovalStore loanStore)
    {
        _loanStore = loanStore;
    }

    public async Task<IReadOnlyList<AuditEntryDto>> Handle(GetAuditQuery request, CancellationToken cancellationToken)
    {
        var loan = await _loanStore.GetByIdAsync(request.LoanId)
            ?? throw new NotFoundException("LoanApproval", request.LoanId);

        return loan.AuditEntries
            .OrderBy(e => e.Timestamp)
            .Take(100)
            .Select(e => new AuditEntryDto
            {
                ActionType = e.ActionType,
                Timestamp = e.Timestamp,
                Status = e.Status
            })
            .ToList();
    }
}
