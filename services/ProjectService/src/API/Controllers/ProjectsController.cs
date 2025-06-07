using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("internal/{projectId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTaskInternal(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null) return NotFound("Project not found");
        return Ok(project);
    }

    [HttpGet("internal/exists/{projectId}")]
    [AllowAnonymous]
    public async Task<IActionResult> ProjectExistsInternal(Guid projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        return Ok(project != null);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> CreateProject(CreateProjectRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var project = await _projectService.CreateProjectAsync(request, userId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return Ok(project);
    }

    [HttpGet("manager/{projectManagerId}")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjectsByManager(Guid projectManagerId)
    {
        var projects = await _projectService.GetProjectsByManagerIdAsync(projectManagerId);
        return Ok(projects);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var project = await _projectService.UpdateProjectAsync(id, request, userId);
        return Ok(project);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _projectService.DeleteProjectAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{projectId}/teams")]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> AddTeamToProject(Guid projectId, AddProjectTeamRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _projectService.AddTeamToProjectAsync(projectId, request, userId);
        return Ok();
    }

    [HttpGet("team/{teamId}")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByTeam(Guid teamId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var projects = await _projectService.GetProjectsByTeamIdAsync(teamId, userId, userRole);
        return Ok(projects);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
    public async Task<IActionResult> UpdateProjectStatus(Guid id, UpdateProjectStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var project = await _projectService.UpdateProjectStatusAsync(id, request, userId);
        return Ok(project);
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchProjects(string? name, string? status)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var projects = await _projectService.SearchProjectsAsync(name, status, userId, userRole);
        return Ok(projects);
    }
}