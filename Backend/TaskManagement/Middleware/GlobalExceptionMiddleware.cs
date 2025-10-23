using System.Net;
using System.Text.Json;
using TaskManagement.DTOs;
using TaskManagement.Exceptions;

namespace TaskManagement.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            TaskNotFoundException ex => new ErrorResponse
            {
                Type = "TaskNotFound",
                Title = "Task Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = ex.Message,
                TraceId = context.TraceIdentifier,
                Extensions = new Dictionary<string, object> { ["taskId"] = ex.TaskId }
            },
            InvalidTaskException ex => new ErrorResponse
            {
                Type = "InvalidTask",
                Title = "Invalid Task",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                TraceId = context.TraceIdentifier,
                Extensions = !string.IsNullOrEmpty(ex.PropertyName)
                    ? new Dictionary<string, object> { ["propertyName"] = ex.PropertyName }
                    : null
            },
            TaskValidationException ex => new ValidationErrorResponse
            {
                Detail = ex.Message,
                TraceId = context.TraceIdentifier,
                Errors = ex.ValidationErrors
            },
            ArgumentNullException ex => new ErrorResponse
            {
                Type = "InvalidArgument",
                Title = "Invalid Argument",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                TraceId = context.TraceIdentifier
            },
            ArgumentException ex => new ErrorResponse
            {
                Type = "InvalidArgument",
                Title = "Invalid Argument",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = ex.Message,
                TraceId = context.TraceIdentifier
            },
            _ => new ErrorResponse
            {
                Type = "InternalError",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "An error occurred while processing your request",
                TraceId = context.TraceIdentifier
            }
        };

        context.Response.StatusCode = response.Status;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
