namespace Application.DTOs;

public record UpdateUserRequest
{
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string? Role { get; init; }
}