namespace GraphBridge.Shared;

/// <summary>
/// Represents a single error entry in the API response envelope.
/// </summary>
public class ApiError
{
    /// <summary>
    /// The field or parameter that caused the error (empty string if not field-specific).
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    public string Detail { get; set; } = string.Empty;
}
