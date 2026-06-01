namespace Membership.Features.Me.Sessions;

public record MeSessionResponse(
    Guid SessionId,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    Guid? CourseId,
    bool IsManual
);
