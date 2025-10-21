using TaskManagement.DTOs;
using TaskManagement.Models;
using TaskManagement.Repositories;

namespace TaskManagement.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<TaskListResponse> GetAllTasks()
    {
        var tasks = await _taskRepository.GetAll();
        var taskResponses = tasks.Select(MapToTaskResponse);

        return new TaskListResponse
        {
            Tasks = taskResponses,
            Count = taskResponses.Count()
        };
    }

    public async Task<TaskResponse?> GetTaskById(long id)
    {
        if (id <= 0)
            return null;

        var task = await _taskRepository.GetById(id);
        return task != null ? MapToTaskResponse(task) : null;
    }

    public async Task<TaskResponse> CreateTask(CreateTaskRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Task title cannot be empty", nameof(request));

        var task = new Models.Task
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            IsCompleted = false
        };

        var createdTask = await _taskRepository.Create(task);
        return MapToTaskResponse(createdTask);
    }

    public async Task<TaskResponse?> ToggleTaskCompletion(long id)
    {
        if (id <= 0)
            return null;

        var existingTask = await _taskRepository.GetById(id);
        if (existingTask == null)
            return null;

        existingTask.IsCompleted = !existingTask.IsCompleted;
        existingTask.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.Update(existingTask);
        return updatedTask != null ? MapToTaskResponse(updatedTask) : null;
    }

    public async Task<bool> DeleteTask(long id)
    {
        if (id <= 0)
            return false;

        return await _taskRepository.Delete(id);
    }

    private static TaskResponse MapToTaskResponse(Models.Task task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
