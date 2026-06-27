using GraphBridge.Application.Dtos.LoanApprovals;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Queries.GetLoanById;

/// <summary>
/// Query to retrieve a specific loan approval by its ID.
/// </summary>
public class GetLoanByIdQuery : IRequest<LoanApprovalDto>
{
    public Guid LoanId { get; set; }
}
