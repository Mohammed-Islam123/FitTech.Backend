using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Courses.GetEnrolledCourses;

/// <description>
/// Returns courses the authenticated member is enrolled in.
/// Currently returns empty until the Courses service is built in Phase 2.
/// </description>
public class GetEnrolledCoursesHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetEnrolledCoursesResponse>>> Handle(
        GetEnrolledCoursesQuery query,
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        if (currentUserId is null)
        {
            return Error.Unauthorized(
                "Courses.Unauthorized",
                "Authentication required.");
        }

        var member = await context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == currentUserId, ct);

        if (member is null)
        {
            return Error.NotFound(
                "Member.NotFound",
                "No member record found.");
        }

        return new List<GetEnrolledCoursesResponse>();
    }
}
