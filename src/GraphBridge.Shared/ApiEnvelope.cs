namespace GraphBridge.Shared;

/// <summary>
/// Standardised API response wrapper for all endpoints.
/// </summary>
/// <typeparam name="T">The type of the response data payload.</typeparam>
public class ApiEnvelope<T>
{
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the result (max 500 characters).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public List<ApiError> Errors { get; set; } = new();

    /// <summary>
    /// ISO 8601 UTC timestamp of when the response was generated.
    /// </summary>
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    /// <summary>
    /// Unique GUID correlating this response to its request for tracing/support.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Creates a successful API envelope with the given data and message.
    /// </summary>
    public static ApiEnvelope<T> Ok(T data, string message) =>
        new()
        {
            Success = true,
            Data = data,
            Message = Truncate(message),
            Errors = new()
        };

    /// <summary>
    /// Creates a failure API envelope with the given message and error details.
    /// </summary>
    public static ApiEnvelope<T> Fail(string message, List<ApiError> errors) =>
        new()
        {
            Success = false,
            Data = default,
            Message = Truncate(message),
            Errors = errors
        };

    private static string Truncate(string value) =>
        value.Length > 500 ? value[..500] : value;
}
