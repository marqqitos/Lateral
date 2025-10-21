using TaskManagement.DTOs;

namespace TaskManagement.Services;

public interface ITaskService
{
    Task<TaskListResponse> GetAllTasksAsync();
    Task<TaskResponse?> GetTaskByIdAsync(long id);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);
    Task<TaskResponse?> ToggleTaskCompletionAsync(long id);
    Task<bool> DeleteTaskAsync(long id);
}
