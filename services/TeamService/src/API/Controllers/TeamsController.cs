using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }
    
    [HttpGet("internal/exists/{teamId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TeamExistsInternal(Guid teamId)
    {
        var team = await _teamService.GetTeamByIdAsync(teamId, Guid.Empty, ""); // Bỏ qua RBAC
        return Ok(team != null);
    }
    [HttpGet("internal/{teamId}/members/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckTeamMembershipInternal(Guid teamId, Guid userId)
    {
        var isMember = await _teamService.IsTeamMemberAsync(teamId, userId);
        return Ok(isMember);
    }
    [HttpGet("internal/user/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserTeamsInternal(Guid userId)
    {
        var teams = await _teamService.GetUserTeamsAsync(userId);
        return Ok(teams);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var team = await _teamService.CreateTeamAsync(request);
        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeam(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var team = await _teamService.GetTeamByIdAsync(id, userId, userRole);
        return Ok(team);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTeams()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return Ok(teams);
    }

    [HttpPost("members")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AddMember([FromBody] AddMemberRequest request)
    {
        await _teamService.AddMemberAsync(request);
        return Ok();
    }

    [HttpDelete("{teamId}/members/{userId}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> RemoveMember(Guid teamId, Guid userId)
    {
        await _teamService.RemoveMemberAsync(teamId, userId);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _teamService.UpdateTeamAsync(id, request);
        return Ok(team);
    }

    [HttpGet("{teamId}/members")]
    [Authorize]
    public async Task<IActionResult> GetTeamMembers(Guid teamId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var members = await _teamService.GetTeamMembersAsync(teamId, userId, userRole);
        return Ok(members);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        await _teamService.DeleteTeamAsync(id);
        return NoContent();
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserTeams(Guid userId)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;
        if (currentUserRole != "ADMIN" && userId != currentUserId) return Forbid();
        var teams = await _teamService.GetUserTeamsAsync(userId);
        return Ok(teams);
    }

    [HttpGet("{teamId}/members/{userId}")]
    [Authorize]
    public async Task<IActionResult> CheckTeamMembership(Guid teamId, Guid userId)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;
        if (currentUserRole != "ADMIN" && userId != currentUserId) return Forbid();
        var isMember = await _teamService.IsTeamMemberAsync(teamId, userId);
        return Ok(isMember);
    }
}