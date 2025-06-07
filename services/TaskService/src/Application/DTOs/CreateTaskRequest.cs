namespace Application.DTOs;

public record CreateTaskRequest(Guid ProjectId, string Title, string? Description, Guid? AssigneeId, string Status);