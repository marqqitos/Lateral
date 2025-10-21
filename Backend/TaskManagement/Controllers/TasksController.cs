using Microsoft.AspNetCore.Mvc;
using TaskManagement.DTOs;
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
        try
        {
            _logger.LogInformation("Retrieving all tasks");
            var result = await _taskService.GetAllTasks();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving tasks");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetTask(long id)
    {
        try
        {
            _logger.LogInformation("Retrieving task with ID: {TaskId}", id);

            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid task ID" });
            }

            var task = await _taskService.GetTaskById(id);
            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving task {TaskId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the task" });
        }
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
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new task with title: {TaskTitle}", request.Title);

            var createdTask = await _taskService.CreateTask(request);

            return CreatedAtAction(
                nameof(GetTask),
                new { id = createdTask.Id },
                createdTask);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while creating task");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating task");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the task" });
        }
    }

    /// <summary>
    /// Toggle the completion status of a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id}/toggle")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> ToggleTaskCompletion(long id)
    {
        try
        {
            _logger.LogInformation("Toggling completion status for task ID: {TaskId}", id);

            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid task ID" });
            }

            var updatedTask = await _taskService.ToggleTaskCompletion(id);
            if (updatedTask == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling task completion {TaskId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the task" });
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(long id)
    {
        try
        {
            _logger.LogInformation("Deleting task with ID: {TaskId}", id);

            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid task ID" });
            }

            var deleted = await _taskService.DeleteTask(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting task {TaskId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the task" });
        }
    }
}
