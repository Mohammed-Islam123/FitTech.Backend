using Courses.Common.Security;
using Courses.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Courses.Features.Programs.GetEnrolledPrograms;

/// <description>
/// Returns programs the authenticated member is enrolled in, with enrollment timestamps.
/// </description>
public class GetEnrolledProgramsHandler(
    CoursesDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetEnrolledProgramsResponse>>> Handle(
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null || !userAccessor.IsMember)
        {
            return Error.Unauthorized(
                "Programs.Unauthorized",
                "Only members can view their enrolled programs.");
        }

        return await context.ProgramEnrollments
            .AsNoTracking()
            .Where(e => e.MemberId == currentUserId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new GetEnrolledProgramsResponse(
                e.Program.Id,
                e.Program.Name,
                e.Program.Coach.FirstName + " " + e.Program.Coach.LastName,
                e.Program.Description,
                e.EnrolledAt))
            .ToListAsync(ct);
    }
}
