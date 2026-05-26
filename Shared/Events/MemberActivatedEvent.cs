namespace Shared.Events;

public record MemberActivatedEvent(
    Guid MemberId,
    Guid UserId,
    string MemberFullName,
    string Email,
    DateTime ActivatedAt
);
