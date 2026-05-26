namespace Shared.Events;

public record MemberSuspendedEvent(
    Guid MemberId,
    Guid UserId,
    string MemberFullName,
    string Email,
    DateTime SuspendedAt
);
