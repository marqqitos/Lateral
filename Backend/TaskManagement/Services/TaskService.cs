using TaskManagement.DTOs;
using TaskManagement.Exceptions;
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

    public async Task<TaskResponse> GetTaskById(long id)
    {
        if (id <= 0)
            throw new InvalidTaskException(nameof(id), "Task ID must be greater than zero");

        var task = await _taskRepository.GetById(id);
        if (task == null)
            throw new TaskNotFoundException(id);

        return MapToTaskResponse(task);
    }

    public async Task<TaskResponse> CreateTask(CreateTaskRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidTaskException(nameof(request.Title), "Task title cannot be empty");

        var task = new Models.Task
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            IsCompleted = false
        };

        var createdTask = await _taskRepository.Create(task);
        return MapToTaskResponse(createdTask);
    }

    public async Task<TaskResponse> ToggleTaskCompletion(long id)
    {
        if (id <= 0)
            throw new InvalidTaskException(nameof(id), "Task ID must be greater than zero");

        var existingTask = await _taskRepository.GetById(id);
        if (existingTask == null)
            throw new TaskNotFoundException(id);

        existingTask.IsCompleted = !existingTask.IsCompleted;
        existingTask.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.Update(existingTask);
        if (updatedTask == null)
            throw new InvalidOperationException("Failed to update task");

        return MapToTaskResponse(updatedTask);
    }

    public async Task<bool> DeleteTask(long id)
    {
        if (id <= 0)
            throw new InvalidTaskException(nameof(id), "Task ID must be greater than zero");

        var existingTask = await _taskRepository.GetById(id);
        if (existingTask == null)
            throw new TaskNotFoundException(id);

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
