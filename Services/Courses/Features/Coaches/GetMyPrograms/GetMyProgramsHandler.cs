using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetMyPrograms;

public class GetMyProgramsHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetMyProgramsResponse>>> Handle(
        GetMyProgramsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view their programs.");
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

        return await context.Programs
            .AsNoTracking()
            .Where(p => p.CoachId == coach.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new GetMyProgramsResponse(
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
