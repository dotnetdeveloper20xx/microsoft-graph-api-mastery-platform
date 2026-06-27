namespace GraphBridge.Application.Dtos.LoanApprovals;

/// <summary>
/// Represents an audit trail entry for loan approval communication actions.
/// </summary>
public class AuditEntryDto
{
    public string ActionType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}
