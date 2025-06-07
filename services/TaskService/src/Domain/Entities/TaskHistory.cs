namespace Domain.Entities;

public partial class TaskHistory
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid ModifiedBy { get; set; }

    public string ChangeDescription { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public virtual Task Task { get; set; } = null!;
}