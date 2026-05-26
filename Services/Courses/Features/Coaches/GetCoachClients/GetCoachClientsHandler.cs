using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetCoachClients;

public class GetCoachClientsHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetCoachClientsResponse>>> Handle(
        GetCoachClientsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view clients.");
        }

        var coach = await context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CoachId, ct);

        if (coach is null)
        {
            return Error.NotFound("Coach.NotFound", "Coach not found.");
        }

        var enrollments = await context.ProgramEnrollments
            .AsNoTracking()
            .Include(e => e.Program)
            .Where(e => e.Program.CoachId == query.CoachId)
            .ToListAsync(ct);

        var grouped = enrollments
            .GroupBy(e => e.MemberId)
            .Select(g => new GetCoachClientsResponse(
                g.Key,
                $"Member-{g.Key.ToString().Substring(0, 8)}",
                string.Empty,
                string.Empty,
                g.Min(e => e.EnrolledAt),
                g.Select(e => new CourseAssignmentResponse(e.ProgramId, e.Program.Name)).ToList()
            ))
            .ToList();

        return grouped;
    }
}
