namespace TaskManagement.DTOs;

public class TaskListResponse
{
    public IEnumerable<TaskResponse> Tasks { get; set; } = new List<TaskResponse>();
    public int Count { get; set; }
}
