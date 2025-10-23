namespace TaskManagement.Exceptions;

/// <summary>
/// Exception thrown when task validation fails
/// </summary>
public class TaskValidationException : Exception
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public TaskValidationException(Dictionary<string, string[]> validationErrors)
        : base("Task validation failed")
    {
        ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
    }

    public TaskValidationException(string message, Dictionary<string, string[]> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
    }

    public TaskValidationException(string message, Dictionary<string, string[]> validationErrors, Exception innerException)
        : base(message, innerException)
    {
        ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
    }
}
