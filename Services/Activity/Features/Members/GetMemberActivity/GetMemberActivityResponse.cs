namespace Activity.Features.Members.GetMemberActivity;

public record GetMemberActivityResponse(
    Guid SessionId,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    Guid? CourseId,
    bool IsManual
);
