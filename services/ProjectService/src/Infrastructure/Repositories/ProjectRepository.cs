using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ProjectDbContext _context;

    public ProjectRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<Project> GetByIdAsync(Guid id)
    {
        return await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetByManagerIdAsync(Guid projectManagerId)
    {
        return await _context.Projects.AsNoTracking().Where(p => p.ProjectManagerId == projectManagerId).ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await GetByIdAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddTeamAsync(ProjectTeam projectTeam)
    {
        await _context.ProjectTeams.AddAsync(projectTeam);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Project>> GetByTeamIdAsync(Guid teamId)
    {
        return await _context.ProjectTeams.Where(pt => pt.TeamId == teamId).Include(pt => pt.Project)
            .Select(pt => pt.Project).ToListAsync();
    }

    public async Task<IEnumerable<Project>> SearchAsync(string? name, string? status)
    {
        var query = _context.Projects.AsQueryable();
        if (!string.IsNullOrEmpty(name)) query = query.Where(p => p.Name.Contains(name));
        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);
        return await query.ToListAsync();
    }
}