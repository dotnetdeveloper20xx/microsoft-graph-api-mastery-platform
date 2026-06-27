namespace GraphBridge.Application.Dtos.LoanApprovals;

/// <summary>
/// Represents a communication pack generated for a loan approval.
/// </summary>
public class CommunicationPackDto
{
    public EmailContentDto CustomerEmail { get; set; } = new();
    public string InternalNotificationContent { get; set; } = string.Empty;
    public List<string> DocumentChecklist { get; set; } = new();
}
