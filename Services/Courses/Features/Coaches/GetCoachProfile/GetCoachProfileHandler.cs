using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetCoachProfile;

/// <description>
/// Returns the public profile of a coach (no email or phone number).
/// Accessible to all authenticated users.
/// </description>
public class GetCoachProfileHandler(CoursesDbContext context)
{
    public async Task<ErrorOr<GetCoachProfileResponse>> Handle(
        Guid coachId,
        CancellationToken ct)
    {
        var coach = await context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == coachId, ct);

        if (coach is null)
        {
            return Error.NotFound(
                "Coach.NotFound",
                "Coach not found.");
        }

        return new GetCoachProfileResponse(
            CoachId: coach.Id,
            FirstName: coach.FirstName,
            LastName: coach.LastName,
            Bio: coach.Bio,
            Specialties: coach.Specialties,
            ProfilePhotoUrl: coach.ProfilePhotoUrl
        );
    }
}
