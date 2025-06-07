namespace Application.DTOs;

public record UpdateTaskRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Status { get; init; }
}