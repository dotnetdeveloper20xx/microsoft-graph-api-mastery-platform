namespace GraphBridge.Shared.Exceptions;

/// <summary>
/// Thrown when a business rule is violated. Maps to HTTP 422.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException()
        : base("A business rule was violated.")
    {
    }

    public BusinessRuleException(string message)
        : base(message)
    {
    }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
