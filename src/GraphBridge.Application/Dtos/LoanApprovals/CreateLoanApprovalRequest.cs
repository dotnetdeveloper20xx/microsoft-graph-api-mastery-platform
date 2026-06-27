namespace GraphBridge.Application.Dtos.LoanApprovals;

/// <summary>
/// Request model for creating a new loan approval record.
/// </summary>
public class CreateLoanApprovalRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PropertyReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
