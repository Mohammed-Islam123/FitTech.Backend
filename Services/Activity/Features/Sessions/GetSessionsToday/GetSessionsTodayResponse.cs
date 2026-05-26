namespace Activity.Features.Sessions.GetSessionsToday;

public record GetSessionsTodayResponse(
    Guid SessionId,
    Guid MemberId,
    string MemberName,
    string? CardUid,
    Guid? CourseId,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    bool IsActive
);
