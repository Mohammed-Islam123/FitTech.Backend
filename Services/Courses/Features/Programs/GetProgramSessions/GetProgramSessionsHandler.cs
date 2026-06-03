using Courses.Domain;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.GetProgramSessions;

/// <description>
/// Returns all sessions for a program with enrolled members and their attendance status per session.
/// Future sessions (not yet completed) exclude attendance data to avoid unnecessary queries.
/// </description>
public class GetProgramSessionsHandler(CoursesDbContext context)
{
    public async Task<ErrorOr<GetProgramSessionsResponse>> Handle(
        GetProgramSessionsQuery query,
        CancellationToken ct)
    {
        var program = await context.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProgramId, ct);

        if (program is null)
        {
            return Error.NotFound(
                "Program.NotFound",
                "The specified program does not exist.");
        }

        var sessions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.ProgramId == query.ProgramId)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync(ct);

        var enrolledMembers = await context.ProgramEnrollments
            .AsNoTracking()
            .Where(e => e.ProgramId == query.ProgramId)
            .ToListAsync(ct);

        // Only fetch attendance records for completed sessions (past and marked)
        var completedSessionIds = sessions
            .Where(s => s.IsCompleted)
            .Select(s => s.Id)
            .ToList();

        var attendanceRecords = completedSessionIds.Count > 0
            ? await context.AttendanceRecords
                .AsNoTracking()
                .Where(a => completedSessionIds.Contains(a.SessionId))
                .ToListAsync(ct)
            : [];

        var response = sessions.Select(s =>
        {
            var isCompleted = s.IsCompleted;
            var sessionAttendance = isCompleted
                ? attendanceRecords.Where(a => a.SessionId == s.Id).ToList()
                : [];

            var members = enrolledMembers.Select(e =>
            {
                string? status = null;

                if (isCompleted)
                {
                    var record = sessionAttendance
                        .FirstOrDefault(a => a.MemberId == e.MemberId);

                    status = record is not null
                        ? (record.Status == AttendanceStatus.Present ? "Present" : "Absent")
                        : "NotMarked";
                }

                return new MemberSessionResponse(
                    e.MemberId,
                    $"Member-{e.MemberId.ToString().Substring(0, 8)}",
                    status);
            }).ToList();

            return new SessionWithMembersResponse(
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                s.IsCompleted,
                members.Count,
                members);
        }).ToList();

        return new GetProgramSessionsResponse(response);
    }
}
