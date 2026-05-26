namespace Courses.Features.Sessions.MarkAttendance;

public record MarkAttendanceResponse(Guid SessionId, int MarkedCount);
