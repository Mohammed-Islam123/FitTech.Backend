using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetMySessions;

public class GetMySessionsHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetMySessionsResponse>>> Handle(
        GetMySessionsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view sessions.");
        }

        if (userAccessor.UserId is null)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "User identity not found in token.");
        }

        var coach = await context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userAccessor.UserId.Value, ct);

        if (coach is null)
        {
            return Error.NotFound("Coach.NotFound", "Coach not found.");
        }

        var sessionsQuery = context.Sessions
            .AsNoTracking()
            .Include(s => s.Program)
            .Include(s => s.TimeSlot)
            .Where(s => s.Program.CoachId == coach.Id);

        if (query.From is not null)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Date >= query.From.Value);
        }

        if (query.To is not null)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Date <= query.To.Value);
        }

        return await sessionsQuery
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .Select(s => new GetMySessionsResponse(
                s.Id,
                s.ProgramId,
                s.Program.Name,
                s.Date,
                s.StartTime,
                s.EndTime,
                s.IsCompleted,
                s.TimeSlot.Day.ToString(),
                s.TimeSlot.Description))
            .ToListAsync(ct);
    }
}
