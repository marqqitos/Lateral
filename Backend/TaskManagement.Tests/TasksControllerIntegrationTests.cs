using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TaskManagement.Data;
using TaskManagement.DTOs;
using AsyncTask = System.Threading.Tasks.Task;

namespace TaskManagement.Tests;

[TestFixture]
public class TasksControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [SetUp]
    public async AsyncTask SetUp()
    {
        // Clear the in-memory database before each test
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        context.Tasks.RemoveRange(context.Tasks);
        await context.SaveChangesAsync();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    #region GET /api/tasks Tests

    [Test]
    public async AsyncTask GetAllTasks_WhenNoTasks_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TaskListResponse>(content, _jsonOptions);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        Assert.That(result.Tasks, Is.Not.Null);
        Assert.That(result.Tasks.Count(), Is.EqualTo(0));
    }

    [Test]
    public async AsyncTask GetAllTasks_WhenTasksExist_ReturnsTaskList()
    {
        // Arrange - Create some tasks first
        var task1 = new CreateTaskRequest { Title = "Integration Test Task 1", Description = "Test Description 1" };
        var task2 = new CreateTaskRequest { Title = "Integration Test Task 2", Description = "Test Description 2" };

        await _client.PostAsJsonAsync("/api/tasks", task1);
        await _client.PostAsJsonAsync("/api/tasks", task2);

        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TaskListResponse>(content, _jsonOptions);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Tasks.Count(), Is.EqualTo(2));

        var tasks = result.Tasks.ToList();
        Assert.That(tasks.Any(t => t.Title == "Integration Test Task 1"), Is.True);
        Assert.That(tasks.Any(t => t.Title == "Integration Test Task 2"), Is.True);
    }

    #endregion

    #region GET /api/tasks/{id} Tests

    [Test]
    public async AsyncTask GetTask_WhenTaskExists_ReturnsTask()
    {
        // Arrange - Create a task first
        var createRequest = new CreateTaskRequest { Title = "Test Task", Description = "Test Description" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdTask.Id));
        Assert.That(result.Title, Is.EqualTo("Test Task"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.IsCompleted, Is.False);
    }

    [Test]
    public async AsyncTask GetTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse.Type, Is.EqualTo("TaskNotFound"));
        Assert.That(errorResponse.Title, Is.EqualTo("Task Not Found"));
        Assert.That(errorResponse.Status, Is.EqualTo(404));
        Assert.That(errorResponse.Detail, Contains.Substring("Task with ID 999 was not found"));
        Assert.That(errorResponse.Extensions, Contains.Key("taskId"));
    }

    [Test]
    public async AsyncTask GetTask_WhenIdIsInvalid_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/0");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse.Type, Is.EqualTo("InvalidTask"));
        Assert.That(errorResponse.Title, Is.EqualTo("Invalid Task"));
        Assert.That(errorResponse.Status, Is.EqualTo(400));
        Assert.That(errorResponse.Detail, Contains.Substring("Task ID must be greater than zero"));
    }

    #endregion

    #region POST /api/tasks Tests

    [Test]
    public async AsyncTask CreateTask_WhenValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Integration Test Task",
            Description = "New Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Title, Is.EqualTo("New Integration Test Task"));
        Assert.That(result.Description, Is.EqualTo("New Test Description"));
        Assert.That(result.IsCompleted, Is.False);
        Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));

        // Verify Location header
        Assert.That(response.Headers.Location, Is.Not.Null);
        Assert.That(response.Headers.Location!.ToString(), Contains.Substring($"/api/Tasks/{result.Id}"));
    }

    [Test]
    public async AsyncTask CreateTask_WhenTitleIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "", Description = "Test Description" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // ASP.NET Core returns standard ValidationProblemDetails for model validation failures
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Contains.Substring("Title"));
        Assert.That(content, Contains.Substring("required") | Contains.Substring("empty"));
    }

    [Test]
    public async AsyncTask CreateTask_WhenTitleIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var requestJson = """{"description": "Test Description"}""";
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tasks", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async AsyncTask CreateTask_WhenTitleExceedsMaxLength_ReturnsBadRequest()
    {
        // Arrange
        var longTitle = new string('A', 201); // Exceeds 200 character limit
        var request = new CreateTaskRequest { Title = longTitle, Description = "Test Description" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async AsyncTask CreateTask_WhenDescriptionExceedsMaxLength_ReturnsBadRequest()
    {
        // Arrange
        var longDescription = new string('A', 1001); // Exceeds 1000 character limit
        var request = new CreateTaskRequest { Title = "Test Task", Description = longDescription };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async AsyncTask CreateTask_WhenDescriptionIsNull_CreatesTaskSuccessfully()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "Task Without Description", Description = null };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Task Without Description"));
        Assert.That(result.Description, Is.Null);
    }

    [Test]
    public async AsyncTask CreateTask_WhenRequestIsInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var invalidJson = """{"title": "Test", "invalid": }""";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/tasks", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    #endregion

    #region PATCH /api/tasks/{id}/toggle Tests

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskExists_TogglesStatus()
    {
        // Arrange - Create a task first
        var createRequest = new CreateTaskRequest { Title = "Toggle Test Task", Description = "Test Description" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        // Act - Toggle completion
        var response = await _client.PatchAsync($"/api/tasks/{createdTask!.Id}/toggle", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(createdTask.Id));
        Assert.That(result.IsCompleted, Is.True); // Should be toggled to true
        Assert.That(result.UpdatedAt, Is.GreaterThan(result.CreatedAt));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskIsCompleted_TogglesBackToIncomplete()
    {
        // Arrange - Create and toggle a task to completed first
        var createRequest = new CreateTaskRequest { Title = "Toggle Back Test Task" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        // Toggle to completed
        await _client.PatchAsync($"/api/tasks/{createdTask!.Id}/toggle", null);

        // Act - Toggle back to incomplete
        var response = await _client.PatchAsync($"/api/tasks/{createdTask.Id}/toggle", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsCompleted, Is.False); // Should be toggled back to false
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.PatchAsync("/api/tasks/999/toggle", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse.Type, Is.EqualTo("TaskNotFound"));
        Assert.That(errorResponse.Title, Is.EqualTo("Task Not Found"));
        Assert.That(errorResponse.Status, Is.EqualTo(404));
        Assert.That(errorResponse.Detail, Contains.Substring("Task with ID 999 was not found"));
        Assert.That(errorResponse.Extensions, Contains.Key("taskId"));
    }

    [Test]
    public async AsyncTask ToggleTaskCompletion_WhenIdIsInvalid_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PatchAsync("/api/tasks/0/toggle", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse.Type, Is.EqualTo("InvalidTask"));
        Assert.That(errorResponse.Title, Is.EqualTo("Invalid Task"));
        Assert.That(errorResponse.Status, Is.EqualTo(400));
        Assert.That(errorResponse.Detail, Contains.Substring("Task ID must be greater than zero"));
    }

    #endregion

    #region DELETE /api/tasks/{id} Tests

    [Test]
    public async AsyncTask DeleteTask_WhenTaskExists_ReturnsNoContent()
    {
        // Arrange - Create a task first
        var createRequest = new CreateTaskRequest { Title = "Delete Test Task", Description = "To be deleted" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify task is actually deleted
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async AsyncTask DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse.Type, Is.EqualTo("TaskNotFound"));
        Assert.That(errorResponse.Title, Is.EqualTo("Task Not Found"));
        Assert.That(errorResponse.Status, Is.EqualTo(404));
        Assert.That(errorResponse.Detail, Contains.Substring("Task with ID 999 was not found"));
        Assert.That(errorResponse.Extensions, Contains.Key("taskId"));
    }

    [Test]
    public async AsyncTask DeleteTask_WhenIdIsInvalid_ReturnsBadRequest()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/0");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse.Type, Is.EqualTo("InvalidTask"));
        Assert.That(errorResponse.Title, Is.EqualTo("Invalid Task"));
        Assert.That(errorResponse.Status, Is.EqualTo(400));
        Assert.That(errorResponse.Detail, Contains.Substring("Task ID must be greater than zero"));
    }

    #endregion

    #region End-to-End Workflow Tests

    [Test]
    public async AsyncTask FullTaskLifecycle_CreateToggleDelete_WorksCorrectly()
    {
        // 1. Create a task
        var createRequest = new CreateTaskRequest
        {
            Title = "Lifecycle Test Task",
            Description = "Full lifecycle test"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(createdTask!.IsCompleted, Is.False);

        // 2. Verify task appears in list
        var listResponse = await _client.GetAsync("/api/tasks");
        var taskList = await listResponse.Content.ReadFromJsonAsync<TaskListResponse>(_jsonOptions);
        Assert.That(taskList!.Tasks.Any(t => t.Id == createdTask.Id), Is.True);

        // 3. Toggle task to completed
        var toggleResponse = await _client.PatchAsync($"/api/tasks/{createdTask.Id}/toggle", null);
        Assert.That(toggleResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var toggledTask = await toggleResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(toggledTask!.IsCompleted, Is.True);

        // 4. Get task by ID to verify status
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var retrievedTask = await getResponse.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
        Assert.That(retrievedTask!.IsCompleted, Is.True);

        // 5. Delete the task
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // 6. Verify task is no longer in list
        var finalListResponse = await _client.GetAsync("/api/tasks");
        var finalTaskList = await finalListResponse.Content.ReadFromJsonAsync<TaskListResponse>(_jsonOptions);
        Assert.That(finalTaskList!.Tasks.Any(t => t.Id == createdTask.Id), Is.False);
    }

    [Test]
    public async AsyncTask MultipleTasksManagement_WorksCorrectly()
    {
        // Create multiple tasks
        var tasks = new List<CreateTaskRequest>
        {
            new() { Title = "Task 1", Description = "First task" },
            new() { Title = "Task 2", Description = "Second task" },
            new() { Title = "Task 3", Description = "Third task" }
        };

        var createdTasks = new List<TaskResponse>();
        foreach (var task in tasks)
        {
            var response = await _client.PostAsJsonAsync("/api/tasks", task);
            var createdTask = await response.Content.ReadFromJsonAsync<TaskResponse>(_jsonOptions);
            createdTasks.Add(createdTask!);
        }

        // Verify all tasks in list
        var listResponse = await _client.GetAsync("/api/tasks");
        var taskList = await listResponse.Content.ReadFromJsonAsync<TaskListResponse>(_jsonOptions);
        Assert.That(taskList!.Count, Is.EqualTo(3));

        // Toggle some tasks
        await _client.PatchAsync($"/api/tasks/{createdTasks[0].Id}/toggle", null);
        await _client.PatchAsync($"/api/tasks/{createdTasks[2].Id}/toggle", null);

        // Verify updated list shows correct completion status
        var updatedListResponse = await _client.GetAsync("/api/tasks");
        var updatedTaskList = await updatedListResponse.Content.ReadFromJsonAsync<TaskListResponse>(_jsonOptions);

        var updatedTasks = updatedTaskList!.Tasks.ToList();
        var task1 = updatedTasks.First(t => t.Id == createdTasks[0].Id);
        var task2 = updatedTasks.First(t => t.Id == createdTasks[1].Id);
        var task3 = updatedTasks.First(t => t.Id == createdTasks[2].Id);

        Assert.That(task1.IsCompleted, Is.True);
        Assert.That(task2.IsCompleted, Is.False);
        Assert.That(task3.IsCompleted, Is.True);
    }

    #endregion

    #region Content-Type and Headers Tests

    [Test]
    public async AsyncTask GetAllTasks_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async AsyncTask CreateTask_WithCorrectContentType_AcceptsRequest()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "Content Type Test Task" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async AsyncTask CreateTask_WithIncorrectContentType_ReturnsBadRequest()
    {
        // Arrange
        var jsonContent = """{"title": "Test Task", "description": "Test"}""";
        var content = new StringContent(jsonContent, Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PostAsync("/api/tasks", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }

    #endregion
}
