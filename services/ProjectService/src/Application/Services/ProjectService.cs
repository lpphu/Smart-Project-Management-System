using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICacheService _cacheService;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ITeamServiceClient _teamServiceClient;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMapper _mapper;

    public ProjectService(IProjectRepository projectRepository, ICacheService cacheService,
        IUserServiceClient userServiceClient, ITeamServiceClient teamServiceClient, IMessagePublisher messagePublisher,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _cacheService = cacheService;
        _userServiceClient = userServiceClient;
        _teamServiceClient = teamServiceClient;
        _messagePublisher = messagePublisher;
        _mapper = mapper;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, Guid currentUserId)
    {
        bool userExists = await _userServiceClient.UserExistsAsync(request.ProjectManagerId);
        if (!userExists) throw new KeyNotFoundException("Project Manager not found");
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ProjectManagerId = request.ProjectManagerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _projectRepository.AddAsync(project);
        var projectDto = _mapper.Map<ProjectDto>(project);
        await _cacheService.SetAsync($"project:{project.Id}", projectDto);
        await _cacheService.RemoveAsync($"projects:manager:{request.ProjectManagerId}");
        await _messagePublisher.PublishAsync("project.created",
            new
            {
                ProjectId = project.Id,
                Name = project.Name,
                ProjectManagerId = project.ProjectManagerId,
                CreatedAt = project.CreatedAt
            });
        return projectDto;
    }

    public async Task<ProjectDto> GetProjectByIdAsync(Guid id)
    {
        var cacheKey = $"project:{id}";
        var cachedProject = await _cacheService.GetAsync<ProjectDto>(cacheKey);
        if (cachedProject != null) return cachedProject;
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");
        var projectDto = _mapper.Map<ProjectDto>(project);
        await _cacheService.SetAsync(cacheKey, projectDto);
        return projectDto;
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByManagerIdAsync(Guid projectManagerId)
    {
        var cacheKey = $"projects:manager:{projectManagerId}";
        var cachedProjects = await _cacheService.GetAsync<IEnumerable<ProjectDto>>(cacheKey);
        if (cachedProjects != null) return cachedProjects;
        bool userExists = await _userServiceClient.UserExistsAsync(projectManagerId);
        if (!userExists) throw new KeyNotFoundException("Project Manager not found");
        var projects = await _projectRepository.GetByManagerIdAsync(projectManagerId);
        var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
        await _cacheService.SetAsync(cacheKey, projectDtos);
        return projectDtos;
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");
        var user = await _userServiceClient.UserExistsAsync(currentUserId);
        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepository.UpdateAsync(project);
        var projectDto = _mapper.Map<ProjectDto>(project);
        await _cacheService.SetAsync($"project:{project.Id}", projectDto);
        await _cacheService.RemoveAsync($"projects:manager:{project.ProjectManagerId}");
        await _messagePublisher.PublishAsync("project.updated",
            new
            {
                ProjectId = project.Id,
                Name = project.Name,
                Description = project.Description,
                UpdatedAt = project.UpdatedAt
            });
        return projectDto;
    }

    public async Task DeleteProjectAsync(Guid id, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");
        await _projectRepository.DeleteAsync(id);
        await _cacheService.RemoveAsync($"project:{id}");
        await _cacheService.RemoveAsync($"projects:manager:{project.ProjectManagerId}");
        await _messagePublisher.PublishAsync("project.deleted", new { ProjectId = id, DeletedBy = currentUserId });
    }

    public async Task AddTeamToProjectAsync(Guid projectId, AddProjectTeamRequest request, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        var projectTeam = new ProjectTeam { ProjectId = projectId, TeamId = request.TeamId };
        await _projectRepository.AddTeamAsync(projectTeam);
        await _cacheService.RemoveAsync($"project:{projectId}");
        await _cacheService.RemoveAsync($"projects:manager:{project.ProjectManagerId}");
        await _messagePublisher.PublishAsync("project.team.added",
            new { ProjectId = projectId, TeamId = request.TeamId, AddedBy = currentUserId });
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByTeamIdAsync(Guid teamId, Guid currentUserId,
        string userRole)
    {
        var cacheKey = $"projects:team:{teamId}";
        var cachedProjects = await _cacheService.GetAsync<IEnumerable<ProjectDto>>(cacheKey);
        if (cachedProjects != null) return cachedProjects;
        bool teamExists = await _teamServiceClient.TeamExistsAsync(teamId);
        if (!teamExists) throw new KeyNotFoundException("Team not found");
        if (userRole != "ADMIN")
        {
            bool isTeamMember = await _teamServiceClient.IsTeamMemberAsync(teamId, currentUserId);
            if (!isTeamMember) throw new UnauthorizedAccessException("You are not a member of this team");
        }

        var projects = await _projectRepository.GetByTeamIdAsync(teamId);
        var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
        await _cacheService.SetAsync(cacheKey, projectDtos, TimeSpan.FromMinutes(5));
        return projectDtos;
    }

    public async Task<ProjectDto> UpdateProjectStatusAsync(Guid id, UpdateProjectStatusRequest request,
        Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");
        if (!new[] { "Planning", "InProgress", "Completed" }.Contains(request.Status))
            throw new ArgumentException("Invalid status");
        project.Status = request.Status;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepository.UpdateAsync(project);
        var projectDto = _mapper.Map<ProjectDto>(project);
        await _cacheService.SetAsync($"project:{project.Id}", projectDto);
        await _cacheService.RemoveAsync($"projects:manager:{project.ProjectManagerId}");
        await _messagePublisher.PublishAsync("project.status.updated",
            new { ProjectId = id, Status = request.Status, UpdatedBy = currentUserId });
        return projectDto;
    }

    public async Task<IEnumerable<ProjectDto>> SearchProjectsAsync(string? name, string? status, Guid currentUserId,
        string userRole)
    {
        var cacheKey = $"projects:search:name:{name ?? "all"}:status:{status ?? "all"}";
        var cachedProjects = await _cacheService.GetAsync<IEnumerable<ProjectDto>>(cacheKey);
        if (cachedProjects != null) return cachedProjects;
        var projects = await _projectRepository.SearchAsync(name, status);
        if (userRole != "ADMIN")
        {
            var user = await _userServiceClient.GetUserByIdAsync(currentUserId);
            if (user.Role == "PROJECT_MANAGER")
                projects = projects.Where(p => p.ProjectManagerId == currentUserId);
            else if (user.Role == "TEAM_MEMBER")
            {
                var userTeams = await _teamServiceClient.GetUserTeamsAsync(currentUserId); // Giả định API
                var teamIds = userTeams.Select(t => t.Id).ToList();
                projects = projects.Where(p => p.ProjectTeams.Any(pt => teamIds.Contains(pt.TeamId)));
            }
        }

        var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
        await _cacheService.SetAsync(cacheKey, projectDtos, TimeSpan.FromMinutes(5));
        return projectDtos;
    }
}