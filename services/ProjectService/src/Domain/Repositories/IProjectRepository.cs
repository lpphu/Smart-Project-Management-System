using Domain.Entities;

namespace Domain.Repositories;

public interface IProjectRepository
{
    Task<Project> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetByManagerIdAsync(Guid projectManagerId);
    Task<IEnumerable<Project>> GetByTeamIdAsync(Guid teamId);
    Task<IEnumerable<Project>> SearchAsync(string? name, string? status);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
    Task AddTeamAsync(ProjectTeam projectTeam);
}