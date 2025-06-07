namespace Application.DTOs;

public record TeamCreatedEvent
{
    public Guid TeamId { get; init; }
    public string Name { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record MemberAddedEvent
{
    public Guid TeamId { get; init; }
    public Guid UserId { get; init; }
    public DateTime AddedAt { get; init; }
}

public record TeamUpdatedEvent
{
    public Guid TeamId { get; init; }
    public string Name { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record TeamDeletedEvent
{
    public Guid TeamId { get; init; }
    public DateTime DeletedAt { get; init; }
}

public record MemberRemovedEvent
{
    public Guid TeamId { get; init; }
    public Guid UserId { get; init; }
    public DateTime RemovedAt { get; init; }
}