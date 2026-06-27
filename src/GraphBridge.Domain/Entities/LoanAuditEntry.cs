namespace GraphBridge.Domain.Entities;

public class LoanAuditEntry
{
    public string ActionType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}
