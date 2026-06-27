namespace GraphBridge.Application.Dtos.Productivity;

/// <summary>
/// Represents a structured context package suitable for AI consumption,
/// containing sections for calendar, emails, tasks, and documents.
/// </summary>
public class AiContextPackageDto
{
    public object Calendar { get; set; } = new();
    public object Emails { get; set; } = new();
    public object Tasks { get; set; } = new();
    public object Documents { get; set; } = new();
}
