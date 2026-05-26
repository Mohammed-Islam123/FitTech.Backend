using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Courses.GetAvailableCourses;

/// <description>
/// Returns courses the authenticated member is NOT enrolled in.
/// Currently returns empty until the Courses service is built in Phase 2.
/// </description>
public class GetAvailableCoursesHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetAvailableCoursesResponse>>> Handle(
        GetAvailableCoursesQuery query,
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

        return new List<GetAvailableCoursesResponse>();
    }
}
