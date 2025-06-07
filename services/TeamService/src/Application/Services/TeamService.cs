using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ICacheService _cacheService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMapper _mapper;

    public TeamService(ITeamRepository teamRepository, IUserServiceClient userServiceClient, ICacheService cacheService,
        IMessagePublisher messagePublisher, IMapper mapper)
    {
        _teamRepository = teamRepository;
        _userServiceClient = userServiceClient;
        _cacheService = cacheService;
        _messagePublisher = messagePublisher;
        _mapper = mapper;
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(), Name = request.Name, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        await _teamRepository.AddAsync(team);
        var teamDto = _mapper.Map<TeamDto>(team);
        await _cacheService.SetAsync($"team:{team.Id}", teamDto);
        await _cacheService.RemoveAsync("teams:all");
        var teamCreatedEvent = new TeamCreatedEvent { TeamId = team.Id, Name = team.Name, CreatedAt = team.CreatedAt };
        await _messagePublisher.PublishAsync("team.created", teamCreatedEvent);
        return teamDto;
    }

    public async Task<TeamDto> GetTeamByIdAsync(Guid id, Guid currentUserId, string userRole)
    {
        var cacheKey = $"team:{id}";
        var cachedTeam = await _cacheService.GetAsync<TeamDto>(cacheKey);
        if (cachedTeam != null) return cachedTeam;
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null) throw new KeyNotFoundException("Team not found");
        if (userRole != "ADMIN" && !team.TeamMembers.Any(m => m.UserId == currentUserId))
            throw new UnauthorizedAccessException("You are not a member of this team");
        var teamDto = _mapper.Map<TeamDto>(team);
        await _cacheService.SetAsync(cacheKey, teamDto);
        return teamDto;
    }

    public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
    {
        var cacheKey = "teams:all";
        var cachedTeams = await _cacheService.GetAsync<IEnumerable<TeamDto>>(cacheKey);
        if (cachedTeams != null) return cachedTeams;
        var teams = await _teamRepository.GetAllAsync();
        var teamDtos = _mapper.Map<IEnumerable<TeamDto>>(teams);
        await _cacheService.SetAsync(cacheKey, teamDtos);
        return teamDtos;
    }

    public async Task AddMemberAsync(AddMemberRequest request)
    {
        var team = await _teamRepository.GetByIdAsync(request.TeamId);
        if (team == null) throw new KeyNotFoundException("Team not found");
        bool userExists = await _userServiceClient.UserExistsAsync(request.UserId);
        if (!userExists) throw new KeyNotFoundException("User not found");
        if (team.TeamMembers.Any(m => m.UserId == request.UserId))
            throw new InvalidOperationException("User is already a member of the team");
        var member = new TeamMember { TeamId = request.TeamId, UserId = request.UserId };
        await _teamRepository.AddMemberAsync(member);
        await _cacheService.RemoveAsync($"team:{request.TeamId}");
        await _cacheService.RemoveAsync("teams:all");
        var memberAddedEvent = new MemberAddedEvent
        {
            TeamId = request.TeamId, UserId = request.UserId, AddedAt = DateTime.UtcNow
        };
        await _messagePublisher.PublishAsync("member.added", memberAddedEvent);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null) throw new KeyNotFoundException("Team not found");
        await _teamRepository.RemoveMemberAsync(teamId, userId);
        await _cacheService.RemoveAsync($"team:{teamId}");
        await _cacheService.RemoveAsync("teams:all");
    }

    public async Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamRequest request)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null) throw new KeyNotFoundException("Team not found");
        team.Name = request.Name;
        team.UpdatedAt = DateTime.UtcNow;
        await _teamRepository.UpdateAsync(team);
        var teamDto = _mapper.Map<TeamDto>(team);
        await _cacheService.SetAsync($"team:{team.Id}", teamDto);
        await _cacheService.RemoveAsync("teams:all");
        var teamUpdatedEvent = new TeamUpdatedEvent
        {
            TeamId = id,
            Name = request.Name,
            UpdatedAt = team.UpdatedAt
        };
        await _messagePublisher.PublishAsync("team.updated", teamUpdatedEvent);
        return teamDto;
    }

    public async Task DeleteTeamAsync(Guid id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null) throw new KeyNotFoundException("Team not found");
        await _teamRepository.DeleteAsync(id);
        await _cacheService.RemoveAsync($"team:{id}");
        await _cacheService.RemoveAsync("teams:all");
        var teamDeletedEvent = new TeamDeletedEvent
        {
            TeamId = id,
            DeletedAt = DateTime.UtcNow
        };
        await _messagePublisher.PublishAsync("team.deleted", teamDeletedEvent);
    }

    public async Task<IEnumerable<TeamDto>> GetUserTeamsAsync(Guid userId)
    {
        var cacheKey = $"teams:user:{userId}";
        var cachedTeams = await _cacheService.GetAsync<IEnumerable<TeamDto>>(cacheKey);
        if (cachedTeams != null)
            return cachedTeams;
        var teams = await _teamRepository.GetByUserIdAsync(userId);
        var teamDtos = new List<TeamDto>();

        foreach (var team in teams)
        {
            var memberIds = team.TeamMembers?.Select(tm => tm.UserId).ToList() ?? new List<Guid>();
            teamDtos.Add(new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                MemberIds = memberIds
            });
        }
        await _cacheService.SetAsync(cacheKey, teamDtos, TimeSpan.FromSeconds(60));
        return teamDtos;
    }

    public async Task<IEnumerable<UserDto>> GetTeamMembersAsync(Guid teamId, Guid currentUserId, string currentUserRole)
    {
        var cacheKey = $"team:members:{teamId}";
        var cachedMembers = await _cacheService.GetAsync<IEnumerable<UserDto>>(cacheKey);
        if (cachedMembers != null)
            return cachedMembers;
        if (currentUserRole != "Admin")
        {
            var isMember = await _teamRepository.HasMemberAsync(teamId, currentUserId);
            if (!isMember)
                throw new UnauthorizedAccessException("User is not a member of the team or an Admin");
        }

        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException("Team not found");
        var memberIds = await _teamRepository.GetMemberIdsAsync(teamId);
        var members = new List<UserDto>();
        foreach (var userId in memberIds)
        {
                var user = await _userServiceClient.GetUserByIdAsync(userId);
                if (user != null)
                    members.Add(user);
        }
        await _cacheService.SetAsync(cacheKey, members, TimeSpan.FromSeconds(60));
        return members;
    }

    public async Task<bool> IsTeamMemberAsync(Guid teamId, Guid userId)
    {
        return await _teamRepository.HasMemberAsync(teamId, userId);
    }
}