using Courses.Common.Security;
using Courses.Domain;
using Courses.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Sessions.GetSessionHistory;

/// <description>
/// Returns session history for the authenticated member within a date range,
/// including attendance status per session.
/// </description>
public class GetSessionHistoryHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetSessionHistoryResponse>> Handle(
        GetSessionHistoryQuery query,
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null || !userAccessor.IsMember)
        {
            return Error.Unauthorized(
                "Sessions.Unauthorized",
                "Only members can view their session history.");
        }

        var enrolledProgramIds = await context.ProgramEnrollments
            .AsNoTracking()
            .Where(e => e.MemberId == currentUserId)
            .Select(e => e.ProgramId)
            .ToListAsync(ct);

        var items = await context.Sessions
            .AsNoTracking()
            .Where(s => enrolledProgramIds.Contains(s.ProgramId)
                        && s.Date >= query.StartDate
                        && s.Date <= query.EndDate)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                s.IsCompleted,
                s.ProgramId,
                ProgramName = s.Program.Name,
                AttendanceStatusValue = s.AttendanceRecords
                    .Where(a => a.MemberId == currentUserId)
                    .Select(a => (AttendanceStatus?)a.Status)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var sessions = items.Select(i =>
        {
            var status = i.AttendanceStatusValue is null
                ? "NotMarked"
                : i.AttendanceStatusValue == AttendanceStatus.Present
                    ? "Present"
                    : "Absent";

            return new SessionHistoryItem(
                i.Id,
                i.Date,
                i.StartTime,
                i.EndTime,
                i.IsCompleted,
                i.ProgramId,
                i.ProgramName,
                status);
        }).ToList();

        return new GetSessionHistoryResponse(sessions);
    }
}
