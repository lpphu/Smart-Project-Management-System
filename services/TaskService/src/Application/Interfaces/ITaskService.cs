using Application.DTOs;

namespace Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid currentUserId);
    Task<TaskDto> GetTaskByIdAsync(Guid id, Guid currentUserId, string userRole);
    Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(Guid projectId);
    Task<IEnumerable<TaskDto>> GetTasksByAssigneeIdAsync(Guid assigneeId, Guid currentUserId, string userRole);
    Task<TaskDto> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusRequest request, Guid currentUserId, string userRole);
    Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid currentUserId); // Thêm
    Task<IEnumerable<TaskDto>> SearchTasksAsync(Guid? projectId, string? status, Guid? assigneeId, Guid currentUserId, string userRole); // Thêm
    Task<IEnumerable<TaskHistoryDto>> GetTaskHistoryAsync(Guid id, Guid currentUserId, string userRole); // Thêm
}