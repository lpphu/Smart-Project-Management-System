using  Application.DTOs;

namespace  Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task<string> LoginAsync(LoginRequest request);
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task DeleteUserAsync(Guid id, Guid currentUserId);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, Guid currentUserId, string currentUserRole);
    Task<IEnumerable<UserDto>> GetUsersAsync(string? roleFilter);
}