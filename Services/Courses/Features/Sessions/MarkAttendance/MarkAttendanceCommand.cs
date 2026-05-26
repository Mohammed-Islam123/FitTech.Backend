namespace Courses.Features.Sessions.MarkAttendance;

public record MarkAttendanceCommand(Guid SessionId, MarkAttendanceRequest Request);

public record MarkAttendanceRequest(List<AttendanceEntry> Attendance);

public record AttendanceEntry(Guid MemberId, string Status);
