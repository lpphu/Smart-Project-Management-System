namespace DefaultNamespace;

public class TaskCreatedEvent
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid? AssigneeId { get; init; }
}