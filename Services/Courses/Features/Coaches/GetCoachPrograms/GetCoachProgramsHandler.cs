using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetCoachPrograms;

public class GetCoachProgramsHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetCoachProgramsResponse>>> Handle(
        GetCoachProgramsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsInRole("Coach") && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view coach programs.");
        }

        var coach = await context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CoachId, ct);

        if (coach is null)
        {
            return Error.NotFound("Coach.NotFound", "Coach not found.");
        }

        return await context.Programs
            .AsNoTracking()
            .Where(p => p.CoachId == query.CoachId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new GetCoachProgramsResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Level,
                p.ExerciseType,
                p.StartDate,
                p.EndDate,
                p.TotalPrice,
                p.Status.ToString(),
                p.Enrollments.Count))
            .ToListAsync(ct);
    }
}
