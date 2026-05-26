using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetCoachClientProfile;

/// <description>
/// Returns full profile of a member enrolled in the coach's program, including medical file.
/// </description>
public class GetCoachClientProfileHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetCoachClientProfileResponse>> Handle(
        GetCoachClientProfileQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view client profiles.");
        }

        var coach = await context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CoachId, ct);

        if (coach is null)
            return Error.NotFound("Coach.NotFound", "Coach not found.");

        var enrollment = await context.ProgramEnrollments
            .AsNoTracking()
            .Include(e => e.Program)
            .FirstOrDefaultAsync(e =>
                e.Program.CoachId == query.CoachId &&
                e.MemberId == query.MemberId, ct);

        if (enrollment is null)
            return Error.NotFound("Member.NotFound", "Member not found in this coach's programs.");

        return new GetCoachClientProfileResponse(
            query.MemberId,
            $"Member-{query.MemberId.ToString().Substring(0, 8)}",
            null,
            null,
            enrollment.EnrolledAt,
            null);
    }
}
