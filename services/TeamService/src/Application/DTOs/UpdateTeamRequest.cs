namespace Application.DTOs;

public record UpdateTeamRequest
{
    public string Name { get; init; } = string.Empty;
}