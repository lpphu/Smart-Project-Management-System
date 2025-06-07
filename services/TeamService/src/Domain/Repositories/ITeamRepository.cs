using Domain.Entities;

namespace Domain.Repositories;

public interface ITeamRepository
{
    Task<Team> GetByIdAsync(Guid id);
    Task<IEnumerable<Team>> GetAllAsync();
    Task<IEnumerable<Team>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Guid>> GetMemberIdsAsync(Guid teamId);
    Task AddAsync(Team team);
    Task UpdateAsync(Team team);
    Task DeleteAsync(Guid id);
    Task AddMemberAsync(TeamMember member);
    Task RemoveMemberAsync(Guid teamId, Guid userId);
    Task<bool> HasMemberAsync(Guid teamId, Guid userId);
}