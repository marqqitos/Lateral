using TaskManagement.Models;

namespace TaskManagement.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<Models.Task>> GetAll();
    Task<Models.Task?> GetById(long id);
    Task<Models.Task> Create(Models.Task task);
    Task<Models.Task?> Update(Models.Task task);
    Task<bool> Delete(long id);
    Task<bool> Exists(long id);
}
