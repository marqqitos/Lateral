using Moq;
using TaskManagement.DTOs;
using TaskManagement.Exceptions;
using TaskManagement.Repositories;
using TaskManagement.Services;
using AsyncTask = System.Threading.Tasks.Task;

namespace TaskManagement.Tests;

[TestFixture]
public class TaskServiceTests
{
    private Mock<ITaskRepository> _mockRepository;
    private TaskService _taskService;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _taskService = new TaskService(_mockRepository.Object);
    }

    [Test]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TaskService(null!));
    }

    #region GetAllTasks Tests

    [Test]
    public async AsyncTask GetAllTasks_WhenTasksExist_ReturnsTaskListResponse()
    {
        // Arrange
        var tasks = new List<Models.Task>
        {
            new() { Id = 1, Title = "Task 1", Description = "Description 1", IsCompleted = false },
            new() { Id = 2, Title = "Task 2", Description = "Description 2", IsCompleted = true }
        };
        _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllTasks();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Tasks.Count(), Is.EqualTo(2));

        var tasksList = result.Tasks.ToList();
        Assert.That(tasksList[0].Id, Is.EqualTo(1));
        Assert.That(tasksList[0].Title, Is.EqualTo("Task 1"));
        Assert.That(tasksList[1].Id, Is.EqualTo(2));
        Assert.That(tasksList[1].Title, Is.EqualTo("Task 2"));
    }

    [Test]
    public async AsyncTask GetAllTasks_WhenNoTasksExist_ReturnsEmptyTaskListResponse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Models.Task>());

        // Act
        var result = await _taskService.GetAllTasks();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        Assert.That(result.Tasks.Count(), Is.EqualTo(0));
    }

    #endregion

    #region GetTaskById Tests

    [Test]
    public async AsyncTask GetTaskById_WhenTaskExists_ReturnsTaskResponse()
    {
        // Arrange
        var task = new Models.Task
        {
            Id = 1,
            Title = "Test Task",
            Description = "Test Description",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskById(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("Test Task"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.IsCompleted, Is.False);
    }

    [Test]
    public async AsyncTask GetTaskById_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetById(It.IsAny<long>())).ReturnsAsync((Models.Task?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskNotFoundException>(() => _taskService.GetTaskById(1));
        Assert.That(exception.TaskId, Is.EqualTo(1));
    }

    [Test]
    public async AsyncTask GetTaskById_WhenIdIsZero_ThrowsInvalidTaskException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.GetTaskById(0));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
        Assert.That(exception.Message, Contains.Substring("Task ID must be greater than zero"));
        _mockRepository.Verify(r => r.GetById(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async AsyncTask GetTaskById_WhenIdIsNegative_ThrowsInvalidTaskException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.GetTaskById(-1));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
        Assert.That(exception.Message, Contains.Substring("Task ID must be greater than zero"));
        _mockRepository.Verify(r => r.GetById(It.IsAny<long>()), Times.Never);
    }

    #endregion

    #region CreateTask Tests

    [Test]
    public async AsyncTask CreateTask_WhenRequestIsValid_ReturnsTaskResponse()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description"
        };

        var createdTask = new Models.Task
        {
            Id = 1,
            Title = "New Task",
            Description = "New Description",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.Create(It.IsAny<Models.Task>())).ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTask(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("New Task"));
        Assert.That(result.Description, Is.EqualTo("New Description"));
        Assert.That(result.IsCompleted, Is.False);

        _mockRepository.Verify(r => r.Create(It.Is<Models.Task>(t =>
            t.Title == "New Task" &&
            t.Description == "New Description" &&
            t.IsCompleted == false)), Times.Once);
    }

    [Test]
    public async AsyncTask CreateTask_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _taskService.CreateTask(null!));
    }

    [Test]
    public async AsyncTask CreateTask_WhenTitleIsEmpty_ThrowsInvalidTaskException()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "", Description = "Description" };

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.CreateTask(request));
        Assert.That(exception.PropertyName, Is.EqualTo("Title"));
        Assert.That(exception.Message, Contains.Substring("Task title cannot be empty"));
    }

    [Test]
    public async AsyncTask CreateTask_WhenTitleIsWhitespace_ThrowsInvalidTaskException()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "   ", Description = "Description" };

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.CreateTask(request));
        Assert.That(exception.PropertyName, Is.EqualTo("Title"));
        Assert.That(exception.Message, Contains.Substring("Task title cannot be empty"));
    }

    [Test]
    public async AsyncTask CreateTask_WhenTitleHasWhitespace_TrimsTitle()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "  New Task  ",
            Description = "  New Description  "
        };

        var createdTask = new Models.Task
        {
            Id = 1,
            Title = "New Task",
            Description = "New Description",
            IsCompleted = false
        };

        _mockRepository.Setup(r => r.Create(It.IsAny<Models.Task>())).ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTask(request);

        // Assert
        _mockRepository.Verify(r => r.Create(It.Is<Models.Task>(t =>
            t.Title == "New Task" &&
            t.Description == "New Description")), Times.Once);
    }

    [Test]
    public async AsyncTask CreateTask_WhenDescriptionIsNull_CreatesTaskSuccessfully()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = null
        };

        var createdTask = new Models.Task
        {
            Id = 1,
            Title = "New Task",
            Description = null,
            IsCompleted = false
        };

        _mockRepository.Setup(r => r.Create(It.IsAny<Models.Task>())).ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTask(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.Null);
    }

    #endregion

    #region ToggleTaskCompletion Tests

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskExists_TogglesCompletionStatus()
    {
        // Arrange
        var existingTask = new Models.Task
        {
            Id = 1,
            Title = "Test Task",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedTask = new Models.Task
        {
            Id = 1,
            Title = "Test Task",
            IsCompleted = true,
            CreatedAt = existingTask.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync(existingTask);
        _mockRepository.Setup(r => r.Update(It.IsAny<Models.Task>())).ReturnsAsync(updatedTask);

        // Act
        var result = await _taskService.ToggleTaskCompletion(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsCompleted, Is.True);

        _mockRepository.Verify(r => r.Update(It.Is<Models.Task>(t =>
            t.Id == 1 && t.IsCompleted == true)), Times.Once);
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskIsCompleted_TogglesBackToIncomplete()
    {
        // Arrange
        var existingTask = new Models.Task
        {
            Id = 1,
            Title = "Test Task",
            IsCompleted = true
        };

        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync(existingTask);
        _mockRepository.Setup(r => r.Update(It.IsAny<Models.Task>())).ReturnsAsync(existingTask);

        // Act
        var result = await _taskService.ToggleTaskCompletion(1);

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<Models.Task>(t =>
            t.Id == 1 && t.IsCompleted == false)), Times.Once);
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetById(It.IsAny<long>())).ReturnsAsync((Models.Task?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskNotFoundException>(() => _taskService.ToggleTaskCompletion(1));
        Assert.That(exception.TaskId, Is.EqualTo(1));
        _mockRepository.Verify(r => r.Update(It.IsAny<Models.Task>()), Times.Never);
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenIdIsZero_ThrowsInvalidTaskException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.ToggleTaskCompletion(0));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
        Assert.That(exception.Message, Contains.Substring("Task ID must be greater than zero"));
        _mockRepository.Verify(r => r.GetById(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenIdIsNegative_ThrowsInvalidTaskException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.ToggleTaskCompletion(-1));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
        Assert.That(exception.Message, Contains.Substring("Task ID must be greater than zero"));
        _mockRepository.Verify(r => r.GetById(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenUpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingTask = new Models.Task { Id = 1, IsCompleted = false };
        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync(existingTask);
        _mockRepository.Setup(r => r.Update(It.IsAny<Models.Task>())).ReturnsAsync((Models.Task?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.ToggleTaskCompletion(1));
        Assert.That(exception.Message, Contains.Substring("Failed to update task"));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_UpdatesTimestamp()
    {
        // Arrange
        var originalTime = DateTime.UtcNow.AddHours(-1);
        var existingTask = new Models.Task
        {
            Id = 1,
            IsCompleted = false,
            UpdatedAt = originalTime
        };

        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync(existingTask);
        _mockRepository.Setup(r => r.Update(It.IsAny<Models.Task>())).ReturnsAsync(existingTask);

        // Act
        await _taskService.ToggleTaskCompletion(1);

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<Models.Task>(t =>
            t.UpdatedAt > originalTime)), Times.Once);
    }

    #endregion

    #region DeleteTask Tests

    [Test]
    public async AsyncTask DeleteTask_WhenTaskExists_ReturnsTrue()
    {
        // Arrange
        var existingTask = new Models.Task { Id = 1, Title = "Test Task" };
        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync(existingTask);
        _mockRepository.Setup(r => r.Delete(1)).ReturnsAsync(true);

        // Act
        var result = await _taskService.DeleteTask(1);

        // Assert
        Assert.That(result, Is.True);
        _mockRepository.Verify(r => r.GetById(1), Times.Once);
        _mockRepository.Verify(r => r.Delete(1), Times.Once);
    }

    [Test]
    public async AsyncTask DeleteTask_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetById(1)).ReturnsAsync((Models.Task?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<TaskNotFoundException>(() => _taskService.DeleteTask(1));
        Assert.That(exception.TaskId, Is.EqualTo(1));
        _mockRepository.Verify(r => r.Delete(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async AsyncTask DeleteTask_WhenIdIsZero_ThrowsInvalidTaskException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.DeleteTask(0));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
        Assert.That(exception.Message, Contains.Substring("Task ID must be greater than zero"));
        _mockRepository.Verify(r => r.Delete(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async AsyncTask DeleteTask_WhenIdIsNegative_ThrowsInvalidTaskException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidTaskException>(() => _taskService.DeleteTask(-1));
        Assert.That(exception.PropertyName, Is.EqualTo("id"));
        Assert.That(exception.Message, Contains.Substring("Task ID must be greater than zero"));
        _mockRepository.Verify(r => r.Delete(It.IsAny<long>()), Times.Never);
    }

    #endregion
}
