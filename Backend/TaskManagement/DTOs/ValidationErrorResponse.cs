namespace TaskManagement.DTOs;

/// <summary>
/// Error response for validation failures
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ValidationErrorResponse()
    {
        Type = "ValidationError";
        Title = "One or more validation errors occurred";
        Status = 400;
    }
}
