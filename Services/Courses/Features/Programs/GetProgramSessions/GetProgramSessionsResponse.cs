namespace Courses.Features.Programs.GetProgramSessions;

/// <description>
/// Wrapper containing all sessions for a program with enrolled members and their attendance status.
/// </description>
public record GetProgramSessionsResponse(List<SessionWithMembersResponse> Sessions);

/// <description>
/// A single session with its enrolled members. For future sessions, attendance data is omitted.
/// </description>
public record SessionWithMembersResponse(
    Guid SessionId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsCompleted,
    int EnrolledCount,
    List<MemberSessionResponse> Members
);

/// <description>
/// A member enrolled in the program, with their attendance status for this specific session.
/// null = attendance not yet marked (future session or past but unmarked).
/// "Present" | "Absent" = already marked for completed sessions.
/// "NotMarked" = session was marked completed but this member has no record.
/// </description>
public record MemberSessionResponse(
    Guid MemberId,
    string FullName,
    string? AttendanceStatus
);
