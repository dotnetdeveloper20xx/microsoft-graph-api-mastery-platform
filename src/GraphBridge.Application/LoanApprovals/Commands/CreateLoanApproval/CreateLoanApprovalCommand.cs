using GraphBridge.Application.Dtos.LoanApprovals;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.CreateLoanApproval;

/// <summary>
/// Command to create a new loan approval record.
/// </summary>
public class CreateLoanApprovalCommand : IRequest<LoanApprovalDto>
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PropertyReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
