using GraphBridge.Application.Dtos.LoanApprovals;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Queries.GetAudit;

/// <summary>
/// Query to retrieve the audit trail for a specific loan approval.
/// Returns entries in chronological order, limited to 100 entries.
/// </summary>
public class GetAuditQuery : IRequest<IReadOnlyList<AuditEntryDto>>
{
    public Guid LoanId { get; set; }
}
