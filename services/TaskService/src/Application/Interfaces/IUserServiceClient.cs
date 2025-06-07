
using Application.DTOs;

namespace Application.Interfaces;

public interface IUserServiceClient
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<UserDto> GetUserByIdAsync(Guid userId);
}