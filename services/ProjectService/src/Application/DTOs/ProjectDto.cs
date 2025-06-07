namespace Application.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectManagerId { get; set; }
    public string Status { get; init; } = "Planning";
}