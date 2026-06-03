using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Coaches.UpdateMyProfile;

public class UpdateMyProfileHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<UpdateMyProfileResponse>> Handle(
        UpdateMyProfileCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsCoach && !userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Coaches and Administrators can update their profile.");
        }

        if (userAccessor.UserId is null)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "User identity not found in token.");
        }

        var coach = await context.Coaches
            .FirstOrDefaultAsync(c => c.UserId == userAccessor.UserId.Value, ct);

        if (coach is null)
        {
            return Error.NotFound("Coach.NotFound", "Coach not found.");
        }

        coach.Bio = command.Request.Bio;
        coach.Specialties = command.Request.Specialties;
        coach.ProfilePhotoUrl = command.Request.ProfilePhotoUrl;

        await context.SaveChangesAsync(ct);

        return new UpdateMyProfileResponse(
            coach.Id,
            coach.Bio,
            coach.Specialties,
            coach.ProfilePhotoUrl);
    }
}
