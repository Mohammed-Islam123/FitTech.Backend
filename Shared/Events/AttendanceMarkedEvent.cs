namespace Shared.Events;

public record AttendanceMarkedEvent(
    Guid SessionId,
    Guid ProgramId,
    string ProgramName,
    DateTime MarkedAt,
    int PresentCount,
    int TotalCount
);
