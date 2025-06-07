namespace Application.DTOs;

public record UpdateTaskStatusRequest
{
    public string Status { get; init; } = string.Empty;
}