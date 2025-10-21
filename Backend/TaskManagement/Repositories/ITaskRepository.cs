using TaskManagement.Models;

namespace TaskManagement.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<Models.Task>> GetAllAsync();
    Task<Models.Task?> GetByIdAsync(long id);
    Task<Models.Task> CreateAsync(Models.Task task);
    Task<Models.Task?> UpdateAsync(Models.Task task);
    Task<bool> DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
}
