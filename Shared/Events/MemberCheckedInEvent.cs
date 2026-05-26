namespace Shared.Events;

public record MemberCheckedInEvent(
    Guid MemberId,
    string? CardUid,
    Guid? CourseId,
    DateTime CheckInTime
);
