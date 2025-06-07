namespace Application.DTOs;

public record UpdateProjectStatusRequest
{
    public string Status { get; init; } = string.Empty;
}