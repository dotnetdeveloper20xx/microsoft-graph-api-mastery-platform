namespace GraphBridge.Shared.Exceptions;

/// <summary>
/// Thrown when a requested resource is not found. Maps to HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
        : base("The requested resource was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string resourceType, object identifier)
        : base($"{resourceType} with identifier '{identifier}' was not found.")
    {
        ResourceType = resourceType;
        Identifier = identifier?.ToString() ?? string.Empty;
    }

    public string ResourceType { get; } = string.Empty;
    public string Identifier { get; } = string.Empty;
}
