namespace Application.DTOs;

public record UpdateProjectRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}