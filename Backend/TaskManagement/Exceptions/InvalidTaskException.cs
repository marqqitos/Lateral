namespace TaskManagement.Exceptions;

/// <summary>
/// Exception thrown when task data is invalid
/// </summary>
public class InvalidTaskException : Exception
{
    public string? PropertyName { get; }

    public InvalidTaskException(string message)
        : base(message)
    {
    }

    public InvalidTaskException(string propertyName, string message)
        : base(message)
    {
        PropertyName = propertyName;
    }

    public InvalidTaskException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public InvalidTaskException(string propertyName, string message, Exception innerException)
        : base(message, innerException)
    {
        PropertyName = propertyName;
    }
}
