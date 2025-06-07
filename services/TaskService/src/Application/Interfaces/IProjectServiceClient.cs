
using Application.DTOs;

namespace Application.Interfaces;

public interface IProjectServiceClient
{
    Task<bool> ProjectExistsAsync(Guid projectId);
    Task<ProjectDto> GetProjectByIdAsync(Guid projectId);
}