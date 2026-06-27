namespace GraphBridge.Shared.Exceptions;

/// <summary>
/// Thrown when a Microsoft Graph API call fails. Maps to HTTP 502.
/// Includes the operation name and failure reason for diagnostics.
/// </summary>
public class GraphServiceException : Exception
{
    public GraphServiceException()
        : base("A Graph API service call failed.")
    {
    }

    public GraphServiceException(string message)
        : base(message)
    {
    }

    public GraphServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public GraphServiceException(string operationName, string failureReason)
        : base($"Graph API operation '{operationName}' failed: {failureReason}")
    {
        OperationName = operationName;
        FailureReason = failureReason;
    }

    public GraphServiceException(string operationName, string failureReason, Exception innerException)
        : base($"Graph API operation '{operationName}' failed: {failureReason}", innerException)
    {
        OperationName = operationName;
        FailureReason = failureReason;
    }

    /// <summary>
    /// The name of the Graph API operation that failed.
    /// </summary>
    public string OperationName { get; } = string.Empty;

    /// <summary>
    /// The reason the Graph API call failed.
    /// </summary>
    public string FailureReason { get; } = string.Empty;
}
