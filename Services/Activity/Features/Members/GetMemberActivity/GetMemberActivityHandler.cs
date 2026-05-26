using Activity.Common.Security;
using Activity.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Activity.Features.Members.GetMemberActivity;

public class GetMemberActivityHandler(ActivityDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetMemberActivityResponse>>> Handle(
        GetMemberActivityQuery query, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin && !userAccessor.IsCoach && !userAccessor.IsMember)
            return Error.Unauthorized("Activity.Unauthorized", "Authentication required.");

        return await context.MemberActivities
            .AsNoTracking()
            .Where(a => a.MemberId == query.MemberId)
            .OrderByDescending(a => a.CheckInTime)
            .Select(a => new GetMemberActivityResponse(
                a.Id, a.CheckInTime, a.CheckOutTime, a.CourseId, a.IsManual))
            .ToListAsync(ct);
    }
}
