namespace Courses.Features.Sessions.GetSessionHistory;

/// <description>
/// Flat list of session history items for the authenticated member within a date range.
/// </description>
public record GetSessionHistoryResponse(List<SessionHistoryItem> Sessions);

/// <description>
/// Represents a single session in the member's history, including attendance status.
/// </description>
public record SessionHistoryItem(
    Guid SessionId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsCompleted,
    Guid ProgramId,
    string ProgramName,
    string AttendanceStatus  // "Present" | "Absent" | "NotMarked"
);
