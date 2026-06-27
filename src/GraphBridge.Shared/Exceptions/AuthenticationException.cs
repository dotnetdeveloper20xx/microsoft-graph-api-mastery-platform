namespace GraphBridge.Shared.Exceptions;

/// <summary>
/// Thrown when authentication fails (expired token, invalid credentials, token acquisition failure).
/// Maps to HTTP 401.
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException()
        : base("Authentication failed.")
    {
    }

    public AuthenticationException(string message)
        : base(message)
    {
    }

    public AuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
