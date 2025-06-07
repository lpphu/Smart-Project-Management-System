using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Repositories;
using NetTask = System.Threading.Tasks.Task;
using Task = Domain.Entities.Task;

namespace Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;

    public TaskRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<Task> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Task>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Tasks.AsNoTracking().Where(t => t.ProjectId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<Task>> GetByAssigneeIdAsync(Guid assigneeId)
    {
        return await _context.Tasks.AsNoTracking().Where(t => t.AssigneeId == assigneeId).ToListAsync();
    }

    public async NetTask AddAsync(Task task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async NetTask UpdateAsync(Task task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async NetTask DeleteAsync(Guid id)
    {
        var task = await GetByIdAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Task>> SearchAsync(Guid? projectId, string? status, Guid? assigneeId)
    {
        var query = _context.Tasks.AsQueryable();
        if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
        if (assigneeId.HasValue) query = query.Where(t => t.AssigneeId == assigneeId);
        return await query.ToListAsync();
    }

    public async NetTask AddHistoryAsync(TaskHistory history)
    {
        await _context.TaskHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskHistory>> GetHistoryAsync(Guid taskId)
    {
        return await _context.TaskHistories.Where(h => h.TaskId == taskId).OrderBy(h => h.ModifiedAt).ToListAsync();
    }
}