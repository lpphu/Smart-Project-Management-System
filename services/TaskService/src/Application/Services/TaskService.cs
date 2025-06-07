using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Task = Domain.Entities.Task;

namespace Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectServiceClient _projectServiceClient;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ICacheService _cacheService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMapper _mapper;

    public TaskService(ITaskRepository taskRepository, IProjectServiceClient projectServiceClient,
        IUserServiceClient userServiceClient, ICacheService cacheService, IMessagePublisher messagePublisher,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _projectServiceClient = projectServiceClient;
        _userServiceClient = userServiceClient;
        _cacheService = cacheService;
        _messagePublisher = messagePublisher;
        _mapper = mapper;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid currentUserId)
    {
        bool projectExists = await _projectServiceClient.ProjectExistsAsync(request.ProjectId);
        if (!projectExists) throw new KeyNotFoundException("Project not found");
        if (request.AssigneeId.HasValue)
        {
            bool userExists = await _userServiceClient.UserExistsAsync(request.AssigneeId.Value);
            if (!userExists) throw new KeyNotFoundException("Assignee not found");
        }

        if (!new[] { "ToDo", "InProgress", "Done" }.Contains(request.Status))
            throw new ArgumentException("Invalid status");
        var task = new Task
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _taskRepository.AddAsync(task);
        var taskDto = _mapper.Map<TaskDto>(task);
        await _cacheService.SetAsync($"task:{task.Id}", taskDto);
        await _cacheService.RemoveAsync($"tasks:project:{request.ProjectId}");
        if (request.AssigneeId.HasValue) await _cacheService.RemoveAsync($"tasks:assignee:{request.AssigneeId}");
        await _messagePublisher.PublishAsync("task.created",
            new
            {
                TaskId = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                AssigneeId = task.AssigneeId,
                Status = task.Status,
                CreatedBy = currentUserId,
                CreatedAt = task.CreatedAt
            });
        return taskDto;
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid id, Guid currentUserId, string userRole)
    {
        var cacheKey = $"task:{id}";
        var cachedTask = await _cacheService.GetAsync<TaskDto>(cacheKey);
        if (cachedTask != null) return cachedTask;
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (userRole != "ADMIN" && userRole != "PROJECT_MANAGER" && task.AssigneeId != currentUserId)
            throw new UnauthorizedAccessException("You can only view your own tasks");
        var taskDto = _mapper.Map<TaskDto>(task);
        await _cacheService.SetAsync(cacheKey, taskDto);
        return taskDto;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(Guid projectId)
    {
        var cacheKey = $"tasks:project:{projectId}";
        var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDto>>(cacheKey);
        if (cachedTasks != null) return cachedTasks;
        bool projectExists = await _projectServiceClient.ProjectExistsAsync(projectId);
        if (!projectExists) throw new KeyNotFoundException("Project not found");
        var tasks = await _taskRepository.GetByProjectIdAsync(projectId);
        var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        await _cacheService.SetAsync(cacheKey, taskDtos);
        return taskDtos;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByAssigneeIdAsync(Guid assigneeId, Guid currentUserId,
        string userRole)
    {
        var cacheKey = $"tasks:assignee:{assigneeId}";
        var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDto>>(cacheKey);
        if (cachedTasks != null) return cachedTasks;
        bool userExists = await _userServiceClient.UserExistsAsync(assigneeId);
        if (!userExists) throw new KeyNotFoundException("Assignee not found");
        if (userRole != "ADMIN" && userRole != "PROJECT_MANAGER" && assigneeId != currentUserId)
            throw new UnauthorizedAccessException("You can only view your own tasks");
        var tasks = await _taskRepository.GetByAssigneeIdAsync(assigneeId);
        var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        await _cacheService.SetAsync(cacheKey, taskDtos);
        return taskDtos;
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusRequest request, Guid currentUserId,
        string userRole)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (userRole != "ADMIN" && userRole != "PROJECT_MANAGER" && task.AssigneeId != currentUserId)
            throw new UnauthorizedAccessException("You can only update the status of your own tasks");
        if (!new[] { "ToDo", "InProgress", "Done" }.Contains(request.Status))
            throw new ArgumentException("Invalid status");
        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskId = id,
            ModifiedBy = currentUserId,
            ChangeDescription = $"Status changed from '{task.Status}' to '{request.Status}'",
            ModifiedAt = DateTime.UtcNow
        };
        await _taskRepository.AddHistoryAsync(history);
        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);
        var taskDto = _mapper.Map<TaskDto>(task);
        await _cacheService.SetAsync($"task:{task.Id}", taskDto);
        await _cacheService.RemoveAsync($"tasks:project:{task.ProjectId}");
        if (task.AssigneeId.HasValue) await _cacheService.RemoveAsync($"tasks:assignee:{task.AssigneeId}");
        await _messagePublisher.PublishAsync("task.status.updated",
            new
            {
                TaskId = id,
                ProjectId = task.ProjectId,
                Status = request.Status,
                UpdatedBy = currentUserId,
                UpdatedAt = task.UpdatedAt
            });
        return taskDto;
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid currentUserId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");
        var project = await _projectServiceClient.GetProjectByIdAsync(task.ProjectId);
        var user = await _userServiceClient.GetUserByIdAsync(currentUserId);
        if (user.Role != "ADMIN" && project.ProjectManagerId != currentUserId)
            throw new UnauthorizedAccessException("You can only update tasks for your own projects");
        var changes = new List<string>();
        if (request.Title != null && request.Title != task.Title)
        {
            changes.Add($"Title changed from '{task.Title}' to '{request.Title}'");
            task.Title = request.Title;
        }

        if (request.Description != null && request.Description != task.Description)
        {
            changes.Add($"Description changed from '{task.Description}' to '{request.Description}'");
            task.Description = request.Description;
        }

        if (request.AssigneeId.HasValue && request.AssigneeId != task.AssigneeId)
        {
            bool userExists = await _userServiceClient.UserExistsAsync(request.AssigneeId.Value);
            if (!userExists) throw new KeyNotFoundException("Assignee not found");
            changes.Add($"Assignee changed from '{task.AssigneeId}' to '{request.AssigneeId}'");
            task.AssigneeId = request.AssigneeId;
        }

        if (request.Status != null && request.Status != task.Status)
        {
            if (!new[] { "ToDo", "InProgress", "Done" }.Contains(request.Status))
                throw new ArgumentException("Invalid status");
            changes.Add($"Status changed from '{task.Status}' to '{request.Status}'");
            task.Status = request.Status;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);
        if (changes.Any())
        {
            var history = new TaskHistory
            {
                Id = Guid.NewGuid(),
                TaskId = id,
                ModifiedBy = currentUserId,
                ChangeDescription = string.Join("; ", changes),
                ModifiedAt = DateTime.UtcNow
            };
            await _taskRepository.AddHistoryAsync(history);
            await _messagePublisher.PublishAsync("task.updated",
                new
                {
                    TaskId = id,
                    ProjectId = task.ProjectId,
                    Title = task.Title,
                    Description = task.Description,
                    AssigneeId = task.AssigneeId,
                    Status = task.Status,
                    UpdatedBy = currentUserId,
                    UpdatedAt = task.UpdatedAt,
                    Changes = changes
                });
        }

        var taskDto = _mapper.Map<TaskDto>(task);
        await _cacheService.SetAsync($"task:{task.Id}", taskDto);
        await _cacheService.RemoveAsync($"tasks:project:{task.ProjectId}");
        if (task.AssigneeId.HasValue) await _cacheService.RemoveAsync($"tasks:assignee:{task.AssigneeId}");
        return taskDto;
    }

    public async Task<IEnumerable<TaskDto>> SearchTasksAsync(Guid? projectId, string? status, Guid? assigneeId,
        Guid currentUserId, string userRole)
    {
        var cacheKey =
            $"tasks:search:project:{projectId?.ToString() ?? "all"}:status:{status ?? "all"}:assignee:{assigneeId?.ToString() ?? "all"}";
        var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDto>>(cacheKey);
        if (cachedTasks != null) return cachedTasks;
        var tasks = await _taskRepository.SearchAsync(projectId, status, assigneeId);
        if (userRole != "ADMIN")
        {
            var project = projectId.HasValue ? await _projectServiceClient.GetProjectByIdAsync(projectId.Value) : null;
            if (userRole == "PROJECT_MANAGER")
                tasks = tasks.Where(t =>
                    project == null || t.ProjectId == projectId && project.ProjectManagerId == currentUserId);
            else if (userRole == "TEAM_MEMBER") tasks = tasks.Where(t => t.AssigneeId == currentUserId);
        }

        var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        await _cacheService.SetAsync(cacheKey, taskDtos, TimeSpan.FromMinutes(5));
        return taskDtos;
    }

    public async Task<IEnumerable<TaskHistoryDto>> GetTaskHistoryAsync(Guid id, Guid currentUserId, string userRole)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (userRole != "ADMIN" && userRole != "PROJECT_MANAGER" && task.AssigneeId != currentUserId)
            throw new UnauthorizedAccessException("You can only view history of your own tasks");
        var history = await _taskRepository.GetHistoryAsync(id);
        return _mapper.Map<IEnumerable<TaskHistoryDto>>(history);
    }
}