using TaskManagement.DTOs;

namespace TaskManagement.Services;

public interface ITaskService
{
    Task<TaskListResponse> GetAllTasks();
    Task<TaskResponse?> GetTaskById(long id);
    Task<TaskResponse> CreateTask(CreateTaskRequest request);
    Task<TaskResponse?> ToggleTaskCompletion(long id);
    Task<bool> DeleteTask(long id);
}
