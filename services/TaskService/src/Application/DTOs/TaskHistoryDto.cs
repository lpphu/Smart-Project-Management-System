namespace Application.DTOs;

public class TaskHistoryDto
{
    public Guid Id { get; init; }
    public Guid TaskId { get; init; }
    public Guid ModifiedBy { get; init; }
    public string ChangeDescription { get; init; } = string.Empty;
    public DateTime ModifiedAt { get; init; }
}