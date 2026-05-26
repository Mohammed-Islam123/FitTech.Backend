using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Courses.Features.Sessions.MarkAttendance;

/// <description>
/// Coach marks attendance for a specific session. For each member specifies Present or Absent.
/// </description>
public class MarkAttendanceHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<MarkAttendanceResponse>> Handle(
        MarkAttendanceCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Attendance.Unauthorized",
                "Only Coaches can mark attendance.");
        }

        var session = await context.Sessions
            .Include(s => s.Program)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, ct);

        if (session is null)
        {
            return Error.NotFound("Session.NotFound", "Session not found.");
        }

        var existingRecords = await context.AttendanceRecords
            .Where(a => a.SessionId == command.SessionId)
            .ToListAsync(ct);

        if (existingRecords.Count > 0)
        {
            return Error.Conflict(
                "Attendance.AlreadyMarked",
                "Attendance has already been marked for this session.");
        }

        int markedCount = 0;
        foreach (var entry in command.Request.Attendance)
        {
            var status = entry.Status.Equals("Present", StringComparison.OrdinalIgnoreCase)
                ? AttendanceStatus.Present
                : AttendanceStatus.Absent;

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                Id = Guid.CreateVersion7(),
                SessionId = command.SessionId,
                MemberId = entry.MemberId,
                Status = status
            });
            markedCount++;
        }

        session.IsCompleted = true;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new AttendanceMarkedEvent(
            SessionId: session.Id,
            ProgramId: session.ProgramId,
            ProgramName: session.Program.Name,
            MarkedAt: DateTime.UtcNow,
            PresentCount: command.Request.Attendance.Count(a =>
                a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase)),
            TotalCount: command.Request.Attendance.Count));

        return new MarkAttendanceResponse(session.Id, markedCount);
    }
}
