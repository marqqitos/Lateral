namespace TaskManagement.Exceptions;

/// <summary>
/// Exception thrown when a task is not found
/// </summary>
public class TaskNotFoundException : Exception
{
    public long TaskId { get; }

    public TaskNotFoundException(long taskId)
        : base($"Task with ID {taskId} was not found")
    {
        TaskId = taskId;
    }

    public TaskNotFoundException(long taskId, string message)
        : base(message)
    {
        TaskId = taskId;
    }

    public TaskNotFoundException(long taskId, string message, Exception innerException)
        : base(message, innerException)
    {
        TaskId = taskId;
    }
}
