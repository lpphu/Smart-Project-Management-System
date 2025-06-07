namespace  Application.DTOs;

public record RegisterRequest
{
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string? Role { get; init; }
}