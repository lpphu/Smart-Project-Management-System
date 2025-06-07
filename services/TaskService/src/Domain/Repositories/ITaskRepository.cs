using Domain.Entities;
using Task = Domain.Entities.Task;
using NetTask = System.Threading.Tasks.Task;
namespace Domain.Repositories;

public interface ITaskRepository
{
    Task<Task> GetByIdAsync(Guid id);
    Task<IEnumerable<Task>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<Task>> GetByAssigneeIdAsync(Guid assigneeId);
    Task<IEnumerable<Task>> SearchAsync(Guid? projectId, string? status, Guid? assigneeId);
    NetTask AddAsync(Task task);
    NetTask UpdateAsync(Task task);
    NetTask AddHistoryAsync(TaskHistory history);
    Task<IEnumerable<TaskHistory>> GetHistoryAsync(Guid taskId);
}