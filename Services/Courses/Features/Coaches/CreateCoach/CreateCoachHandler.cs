using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;

namespace Courses.Features.Coaches.CreateCoach;

/// <description>
/// Creates a new coach profile linked to an Identity service user.
/// </description>
public class CreateCoachHandler(CoursesDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<CreateCoachResponse>> Handle(
        CreateCoachCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Coach.Unauthorized",
                "Only Administrators can create coach profiles.");
        }

        var req = command.Request;

        var coach = new Domain.Entities.Coach
        {
            Id = Guid.CreateVersion7(),
            UserId = req.UserId,
            FullName = req.FullName,
            Email = req.Email,
            PhoneNumber = req.PhoneNumber,
            Bio = req.Bio
        };

        context.Coaches.Add(coach);
        await context.SaveChangesAsync(ct);

        return new CreateCoachResponse(coach.Id);
    }
}
