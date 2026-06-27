namespace GraphBridge.Application.Dtos.Graph;

/// <summary>
/// Represents an error indicator for an unavailable section in aggregated responses.
/// </summary>
public class SectionErrorDto
{
    public string Section { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
