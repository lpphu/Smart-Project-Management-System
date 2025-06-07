namespace Domain.Entities;

public partial class ProjectTeam
{
    public Guid ProjectId { get; set; }

    public Guid TeamId { get; set; }

    public virtual Project Project { get; set; } = null!;
}