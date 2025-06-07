using Application.DTOs;

namespace Application.Interfaces;

public interface ITeamService
{
    Task<TeamDto> CreateTeamAsync(CreateTeamRequest request);
    Task<TeamDto> GetTeamByIdAsync(Guid id, Guid currentUserId, string userRole);
    Task<IEnumerable<TeamDto>> GetAllTeamsAsync();
    Task AddMemberAsync(AddMemberRequest request);
    Task RemoveMemberAsync(Guid teamId, Guid userId);
    Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamRequest request);
    Task<IEnumerable<UserDto>> GetTeamMembersAsync(Guid teamId, Guid currentUserId, string userRole);
    Task DeleteTeamAsync(Guid id);
    Task<IEnumerable<TeamDto>> GetUserTeamsAsync(Guid userId);
    Task<bool> IsTeamMemberAsync(Guid teamId, Guid userId);
}