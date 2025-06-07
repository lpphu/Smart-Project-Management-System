using Application.DTOs;

namespace Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, Guid currentUserId);
    Task<ProjectDto> GetProjectByIdAsync(Guid id);
    Task<IEnumerable<ProjectDto>> GetProjectsByManagerIdAsync(Guid projectManagerId);
    Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, Guid currentUserId);
    Task DeleteProjectAsync(Guid id, Guid currentUserId);
    Task AddTeamToProjectAsync(Guid projectId, AddProjectTeamRequest request, Guid currentUserId);
    Task<IEnumerable<ProjectDto>> GetProjectsByTeamIdAsync(Guid teamId, Guid currentUserId, string userRole);
    Task<ProjectDto> UpdateProjectStatusAsync(Guid id, UpdateProjectStatusRequest request, Guid currentUserId);
    Task<IEnumerable<ProjectDto>> SearchProjectsAsync(string? name, string? status, Guid currentUserId, string userRole);
}