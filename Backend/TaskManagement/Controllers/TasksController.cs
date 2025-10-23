using Microsoft.AspNetCore.Mvc;
using TaskManagement.DTOs;
using TaskManagement.Exceptions;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
		ITaskService taskService,
		ILogger<TasksController> logger
	)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    /// <returns>List of all tasks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskListResponse>> GetAllTasks()
    {
        _logger.LogInformation("Retrieving all tasks");
        var result = await _taskService.GetAllTasks();
        return Ok(result);
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetTask(long id)
    {
        _logger.LogInformation("Retrieving task with ID: {TaskId}", id);
        var task = await _taskService.GetTaskById(id);
        return Ok(task);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="request">Task creation request</param>
    /// <returns>Created task</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            throw new TaskValidationException(validationErrors);
        }

        _logger.LogInformation("Creating new task with title: {TaskTitle}", request.Title);
        var createdTask = await _taskService.CreateTask(request);
        return CreatedAtAction(
            nameof(GetTask),
            new { id = createdTask.Id },
            createdTask);
    }

    /// <summary>
    /// Toggle the completion status of a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id}/toggle")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> ToggleTaskCompletion(long id)
    {
        _logger.LogInformation("Toggling completion status for task ID: {TaskId}", id);
        var updatedTask = await _taskService.ToggleTaskCompletion(id);
        return Ok(updatedTask);
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(long id)
    {
        _logger.LogInformation("Deleting task with ID: {TaskId}", id);
        await _taskService.DeleteTask(id);
        return NoContent();
    }
}
