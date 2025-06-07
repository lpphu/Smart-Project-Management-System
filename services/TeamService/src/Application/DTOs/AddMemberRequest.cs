namespace Application.DTOs;

public record AddMemberRequest(Guid TeamId, Guid UserId);