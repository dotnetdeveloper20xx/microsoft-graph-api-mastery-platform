namespace GraphBridge.Domain.Entities;

public class LoanApproval
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PropertyReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool PackGenerated { get; set; }
    public List<LoanAuditEntry> AuditEntries { get; set; } = new();
}
