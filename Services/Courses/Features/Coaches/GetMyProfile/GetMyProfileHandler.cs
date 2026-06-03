using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.GetMyProfile;

public class GetMyProfileHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetMyProfileResponse>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can view their own profile.");
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
            return Error.NotFound("Coach.NotFound", "Coach profile not found.");
        }

        return new GetMyProfileResponse(
            coach.Id,
            coach.UserId,
            coach.FirstName,
            coach.LastName,
            coach.Email,
            coach.PhoneNumber,
            coach.Bio,
            coach.Specialties,
            coach.ProfilePhotoUrl,
            coach.CreatedAt);
    }
}
