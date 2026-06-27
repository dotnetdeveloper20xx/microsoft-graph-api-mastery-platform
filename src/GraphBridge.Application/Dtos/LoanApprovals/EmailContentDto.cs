namespace GraphBridge.Application.Dtos.LoanApprovals;

/// <summary>
/// Represents email content with subject and body.
/// </summary>
public class EmailContentDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
