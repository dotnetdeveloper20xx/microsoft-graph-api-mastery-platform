namespace GraphBridge.Application.Dtos.LoanApprovals;

/// <summary>
/// Represents a loan approval record.
/// </summary>
public class LoanApprovalDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PropertyReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool PackGenerated { get; set; }
}
