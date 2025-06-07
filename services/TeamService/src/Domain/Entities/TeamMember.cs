namespace Domain.Entities;

public partial class TeamMember
{
    public Guid TeamId { get; set; }

    public Guid UserId { get; set; }

    public virtual Team Team { get; set; } = null!;
}