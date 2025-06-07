using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpGet("internal/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserInternal(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }
        return Ok(user);
    }
    [HttpGet("internal/exists/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> UserExistsInternal(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        return Ok(user != null);
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetUsers(string? role)
    {
        var users = await _userService.GetUsersAsync(role);
        return Ok(users);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var token = await _userService.LoginAsync(request);
        return Ok(new { Token = token });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _userService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPost("create-admin")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateAdminUser(RegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _userService.DeleteUserAsync(id, currentUserId);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var user = await _userService.UpdateUserAsync(id, request, currentUserId, currentUserRole);
        return Ok(user);
    }
}