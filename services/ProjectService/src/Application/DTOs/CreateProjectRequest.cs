namespace Application.DTOs;

public record CreateProjectRequest(string Name, string? Description, Guid ProjectManagerId);