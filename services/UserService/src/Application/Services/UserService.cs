using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _userCacheService;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(IUserRepository userRepository, ICacheService userCacheService, IMapper mapper,
        IPasswordHasher<User> passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _userCacheService = userCacheService;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null) throw new InvalidOperationException("Email already exists");
        var validRoles = new[] { "ADMIN", "PROJECT_MANAGER", "TEAM_MEMBER" };
        var role = request.Role ?? "TEAM_MEMBER";
        if (!validRoles.Contains(role)) throw new ArgumentException("Invalid role");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _userRepository.AddAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        await _userCacheService.SetAsync($"user:{user.Id}", userDto);
        return userDto;
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) ==
            PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");
        return _jwtTokenGenerator.GenerateToken(user);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var cacheKey = $"user:{id}";
        var cachedUser = await _userCacheService.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null) return cachedUser;
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        var userDto = _mapper.Map<UserDto>(user);
        await _userCacheService.SetAsync(cacheKey, userDto);
        return userDto;
    }

    public async Task DeleteUserAsync(Guid id, Guid currentUserId)
    {
        if (id == currentUserId) throw new InvalidOperationException("You cannot delete yourself");
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        await _userRepository.DeleteAsync(id);
        await _userCacheService.RemoveAsync($"user:{id}");
        await _userCacheService.RemoveAsync($"user:email:{user.Email}");
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, Guid currentUserId,
        string currentUserRole)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        if (currentUserRole != "ADMIN" && id != currentUserId)
            throw new UnauthorizedAccessException("You can only update your own profile");
        if (request.Role != null && currentUserRole != "ADMIN")
            throw new UnauthorizedAccessException("Only Admin can update roles");
        if (request.Role != null)
        {
            var validRoles = new[] { "ADMIN", "PROJECT_MANAGER", "TEAM_MEMBER" };
            if (!validRoles.Contains(request.Role)) throw new ArgumentException("Invalid role");
            user.Role = request.Role;
        }

        if (request.Username != null) user.Username = request.Username;
        if (request.Email != null)
        {
            if (await _userRepository.GetByEmailAsync(request.Email) != null && request.Email != user.Email)
                throw new InvalidOperationException("Email already exists");
            user.Email = request.Email;
        }

        if (request.Password != null) user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        await _userCacheService.SetAsync($"user:{user.Id}", userDto);
        await _userCacheService.RemoveAsync($"user:email:{user.Email}");
        return userDto;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync(string? roleFilter)
    {
        var cacheKey = $"users:role:{roleFilter ?? "all"}";
        var cachedUsers = await _userCacheService.GetAsync<IEnumerable<UserDto>>(cacheKey);
        if (cachedUsers != null) return cachedUsers;
        var users = string.IsNullOrEmpty(roleFilter)
            ? await _userRepository.GetAllAsync()
            : await _userRepository.GetByRoleAsync(roleFilter);
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        await _userCacheService.SetAsync(cacheKey, userDtos, TimeSpan.FromMinutes(5));
        return userDtos;
    }
}