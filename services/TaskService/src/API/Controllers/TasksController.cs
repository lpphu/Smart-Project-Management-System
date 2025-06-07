using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("internal/exists/{taskId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TaskExistsInternal(Guid taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId, Guid.Empty, "");
        return Ok(task != null);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var task = await _taskService.CreateTaskAsync(request, userId);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var task = await _taskService.GetTaskByIdAsync(id, userId, userRole);
        return Ok(task);
    }

    [HttpGet("project/{projectId}")]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByProject(Guid projectId)
    {
        var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);
        return Ok(tasks);
    }

    [HttpGet("assignee/{assigneeId}")]
    public async Task<IActionResult> GetTasksByAssignee(Guid assigneeId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var tasks = await _taskService.GetTasksByAssigneeIdAsync(assigneeId, userId, userRole);
        return Ok(tasks);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, UpdateTaskStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var task = await _taskService.UpdateTaskStatusAsync(id, request, userId, userRole);
        return Ok(task);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var task = await _taskService.UpdateTaskAsync(id, request, userId);
        return Ok(task);
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchTasks(Guid? projectId, string? status, Guid? assigneeId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var tasks = await _taskService.SearchTasksAsync(projectId, status, assigneeId, userId, userRole);
        return Ok(tasks);
    }

    [HttpGet("{id}/history")]
    [Authorize]
    public async Task<IActionResult> GetTaskHistory(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var history = await _taskService.GetTaskHistoryAsync(id, userId, userRole);
        return Ok(history);
    }
}