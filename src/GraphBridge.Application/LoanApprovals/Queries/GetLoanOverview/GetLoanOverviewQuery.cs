using GraphBridge.Application.Dtos.LoanApprovals;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Queries.GetLoanOverview;

/// <summary>
/// Query to retrieve all loan approval records.
/// </summary>
public class GetLoanOverviewQuery : IRequest<IReadOnlyList<LoanApprovalDto>>
{
}
