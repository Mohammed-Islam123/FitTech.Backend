namespace Shared.Events;

public record MemberCheckedOutEvent(
    Guid MemberId,
    string? CardUid,
    Guid? CourseId,
    DateTime CheckOutTime
);
