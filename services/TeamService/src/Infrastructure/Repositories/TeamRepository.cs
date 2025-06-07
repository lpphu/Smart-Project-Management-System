using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly TeamDbContext _context;

    public TeamRepository(TeamDbContext context)
    {
        _context = context;
    }

    public async Task<Team> GetByIdAsync(Guid id)
    {
        return await _context.Teams.Include(t => t.TeamMembers).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        return await _context.Teams.Include(t => t.TeamMembers).AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(Team team)
    {
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Team team)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var team = await GetByIdAsync(id);
        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddMemberAsync(TeamMember member)
    {
        await _context.TeamMembers.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId)
    {
        var member = await _context.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
        if (member != null)
        {
            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Team>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Teams
            .Where(t => t.TeamMembers.Any(tm => tm.UserId == userId))
            .Include(t => t.TeamMembers)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetMemberIdsAsync(Guid teamId)
    {
        return await _context.TeamMembers
            .Where(tm => tm.TeamId == teamId)
            .Select(tm => tm.UserId)
            .ToListAsync();
    }

    public async Task<bool> HasMemberAsync(Guid teamId, Guid userId)
    {
        return await _context.TeamMembers.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
    }
}