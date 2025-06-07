using Application.DTOs;

namespace Application.Interfaces;

public interface ITeamServiceClient
{
    Task<bool> TeamExistsAsync(Guid teamId);
    Task<bool> IsTeamMemberAsync(Guid teamId, Guid userId);
    Task<IEnumerable<TeamDto>> GetUserTeamsAsync(Guid userId);
}