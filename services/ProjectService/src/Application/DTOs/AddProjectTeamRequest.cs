namespace Application.DTOs;

public record AddProjectTeamRequest
{
    public Guid TeamId { get; init; }
}