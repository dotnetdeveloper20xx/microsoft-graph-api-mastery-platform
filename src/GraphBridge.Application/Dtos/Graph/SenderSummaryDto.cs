namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents a sender's email volume summary.
/// </summary>
public class SenderSummaryDto
{
    public string SenderName { get; set; } = string.Empty;
    public int MessageCount { get; set; }
}
