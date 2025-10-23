using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Controllers;
using TaskManagement.DTOs;
using TaskManagement.Exceptions;
using TaskManagement.Services;
using AsyncTask = System.Threading.Tasks.Task;

namespace TaskManagement.Tests;

[TestFixture]
public class TasksControllerTests
{
    private Mock<ITaskService> _mockTaskService;
    private Mock<ILogger<TasksController>> _mockLogger;
    private TasksController _controller;

    [SetUp]
    public void Setup()
    {
        _mockTaskService = new Mock<ITaskService>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_mockTaskService.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WhenTaskServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TasksController(null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TasksController(_mockTaskService.Object, null!));
    }

    #endregion

    #region GetAllTasks Tests

    [Test]
    public async AsyncTask GetAllTasks_WhenTasksExist_ReturnsOkWithTaskList()
    {
        // Arrange
        var taskListResponse = new TaskListResponse
        {
            Tasks = new List<TaskResponse>
            {
                new() { Id = 1, Title = "Task 1", IsCompleted = false },
                new() { Id = 2, Title = "Task 2", IsCompleted = true }
            },
            Count = 2
        };
        _mockTaskService.Setup(s => s.GetAllTasks()).ReturnsAsync(taskListResponse);

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(taskListResponse));

        _mockTaskService.Verify(s => s.GetAllTasks(), Times.Once);
    }

    [Test]
    public async AsyncTask GetAllTasks_WhenNoTasksExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyResponse = new TaskListResponse { Tasks = new List<TaskResponse>(), Count = 0 };
        _mockTaskService.Setup(s => s.GetAllTasks()).ReturnsAsync(emptyResponse);

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as TaskListResponse;
        Assert.That(response!.Count, Is.EqualTo(0));
    }

    [Test]
    public async AsyncTask GetAllTasks_WhenServiceThrowsException_ExceptionBubbles()
    {
        // Arrange
        _mockTaskService.Setup(s => s.GetAllTasks()).ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _controller.GetAllTasks());
    }

    #endregion

    #region GetTask Tests

    [Test]
    public async AsyncTask GetTask_WhenTaskExists_ReturnsOkWithTask()
    {
        // Arrange
        var taskId = 1L;
        var taskResponse = new TaskResponse
        {
            Id = taskId,
            Title = "Test Task",
            IsCompleted = false
        };
        _mockTaskService.Setup(s => s.GetTaskById(taskId)).ReturnsAsync(taskResponse);

        // Act
        var result = await _controller.GetTask(taskId);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(taskResponse));
    }

    [Test]
    public async AsyncTask GetTask_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        // Arrange
        var taskId = 999L;
        _mockTaskService.Setup(s => s.GetTaskById(taskId)).ThrowsAsync(new TaskNotFoundException(taskId));

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskNotFoundException>(() => _controller.GetTask(taskId));
        Assert.That(exception.TaskId, Is.EqualTo(taskId));
    }

    [Test]
    public async AsyncTask GetTask_WhenIdIsZero_ThrowsInvalidTaskException()
    {
        // Arrange
        _mockTaskService.Setup(s => s.GetTaskById(0)).ThrowsAsync(new InvalidTaskException("id", "Task ID must be greater than zero"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.GetTask(0));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
    }

    [Test]
    public async AsyncTask GetTask_WhenIdIsNegative_ThrowsInvalidTaskException()
    {
        // Arrange
        _mockTaskService.Setup(s => s.GetTaskById(-1)).ThrowsAsync(new InvalidTaskException("id", "Task ID must be greater than zero"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.GetTask(-1));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
    }

    [Test]
    public async AsyncTask GetTask_WhenServiceThrowsException_ExceptionBubbles()
    {
        // Arrange
        _mockTaskService.Setup(s => s.GetTaskById(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _controller.GetTask(1));
    }

    #endregion

    #region CreateTask Tests

    [Test]
    public async AsyncTask CreateTask_WhenRequestIsValid_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description"
        };
        var createdTask = new TaskResponse
        {
            Id = 1,
            Title = "New Task",
            Description = "New Description",
            IsCompleted = false
        };

        _mockTaskService.Setup(s => s.CreateTask(request)).ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(TasksController.GetTask)));
        Assert.That(createdResult.Value, Is.EqualTo(createdTask));

        var routeValues = createdResult.RouteValues;
        Assert.That(routeValues!["id"], Is.EqualTo(1L));
    }

    [Test]
    public async AsyncTask CreateTask_WhenModelStateIsInvalid_ThrowsTaskValidationException()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "", Description = "Test" };
        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskValidationException>(() => _controller.CreateTask(request));
        Assert.That(exception.ValidationErrors, Contains.Key("Title"));
        _mockTaskService.Verify(s => s.CreateTask(It.IsAny<CreateTaskRequest>()), Times.Never);
    }

    [Test]
    public async AsyncTask CreateTask_WhenServiceThrowsInvalidTaskException_ExceptionBubbles()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "Test Task" };
        _mockTaskService.Setup(s => s.CreateTask(request))
            .ThrowsAsync(new InvalidTaskException("Invalid title"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.CreateTask(request));
        Assert.That(exception.Message, Contains.Substring("Invalid title"));
    }

    [Test]
    public async AsyncTask CreateTask_WhenServiceThrowsGeneralException_ExceptionBubbles()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "Test Task" };
        _mockTaskService.Setup(s => s.CreateTask(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _controller.CreateTask(request));
    }

    #endregion

    #region ToggleTaskCompletion Tests

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskExists_ReturnsOkWithUpdatedTask()
    {
        // Arrange
        var taskId = 1L;
        var updatedTask = new TaskResponse
        {
            Id = taskId,
            Title = "Test Task",
            IsCompleted = true
        };
        _mockTaskService.Setup(s => s.ToggleTaskCompletion(taskId)).ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.ToggleTaskCompletion(taskId);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(updatedTask));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        // Arrange
        var taskId = 999L;
        _mockTaskService.Setup(s => s.ToggleTaskCompletion(taskId))
            .ThrowsAsync(new TaskNotFoundException(taskId));

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskNotFoundException>(() => _controller.ToggleTaskCompletion(taskId));
        Assert.That(exception.TaskId, Is.EqualTo(taskId));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenIdIsZero_ThrowsInvalidTaskException()
    {
        // Arrange
        _mockTaskService.Setup(s => s.ToggleTaskCompletion(0))
            .ThrowsAsync(new InvalidTaskException("id", "Task ID must be greater than zero"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.ToggleTaskCompletion(0));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenIdIsNegative_ThrowsInvalidTaskException()
    {
        // Arrange
        _mockTaskService.Setup(s => s.ToggleTaskCompletion(-1))
            .ThrowsAsync(new InvalidTaskException("id", "Task ID must be greater than zero"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.ToggleTaskCompletion(-1));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenServiceThrowsException_ExceptionBubbles()
    {
        // Arrange
        _mockTaskService.Setup(s => s.ToggleTaskCompletion(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _controller.ToggleTaskCompletion(1));
    }

    #endregion

    #region DeleteTask Tests

    [Test]
    public async AsyncTask DeleteTask_WhenTaskExists_ReturnsNoContent()
    {
        // Arrange
        var taskId = 1L;
        _mockTaskService.Setup(s => s.DeleteTask(taskId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        Assert.That(result, Is.TypeOf<NoContentResult>());
        _mockTaskService.Verify(s => s.DeleteTask(taskId), Times.Once);
    }

    [Test]
    public async AsyncTask DeleteTask_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        // Arrange
        var taskId = 999L;
        _mockTaskService.Setup(s => s.DeleteTask(taskId)).ThrowsAsync(new TaskNotFoundException(taskId));

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskNotFoundException>(() => _controller.DeleteTask(taskId));
        Assert.That(exception.TaskId, Is.EqualTo(taskId));
    }

    [Test]
    public async AsyncTask DeleteTask_WhenIdIsZero_ThrowsInvalidTaskException()
    {
        // Arrange
        _mockTaskService.Setup(s => s.DeleteTask(0))
            .ThrowsAsync(new InvalidTaskException("id", "Task ID must be greater than zero"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.DeleteTask(0));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
    }

    [Test]
    public async AsyncTask DeleteTask_WhenIdIsNegative_ThrowsInvalidTaskException()
    {
        // Arrange
        _mockTaskService.Setup(s => s.DeleteTask(-1))
            .ThrowsAsync(new InvalidTaskException("id", "Task ID must be greater than zero"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _controller.DeleteTask(-1));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
    }

    [Test]
    public async AsyncTask DeleteTask_WhenServiceThrowsException_ExceptionBubbles()
    {
        // Arrange
        _mockTaskService.Setup(s => s.DeleteTask(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _controller.DeleteTask(1));
    }

    #endregion

    #region Logging Tests

    [Test]
    public async AsyncTask GetAllTasks_LogsInformationMessage()
    {
        // Arrange
        var taskListResponse = new TaskListResponse { Tasks = new List<TaskResponse>(), Count = 0 };
        _mockTaskService.Setup(s => s.GetAllTasks()).ReturnsAsync(taskListResponse);

        // Act
        await _controller.GetAllTasks();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving all tasks")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async AsyncTask GetTask_LogsInformationWithTaskId()
    {
        // Arrange
        var taskId = 1L;
        var taskResponse = new TaskResponse { Id = taskId, Title = "Test" };
        _mockTaskService.Setup(s => s.GetTaskById(taskId)).ReturnsAsync(taskResponse);

        // Act
        await _controller.GetTask(taskId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieving task with ID: {taskId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async AsyncTask CreateTask_LogsInformationWithTaskTitle()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "New Task" };
        var createdTask = new TaskResponse { Id = 1, Title = "New Task" };
        _mockTaskService.Setup(s => s.CreateTask(request)).ReturnsAsync(createdTask);

        // Act
        await _controller.CreateTask(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating new task with title: New Task")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }



    #endregion
}
