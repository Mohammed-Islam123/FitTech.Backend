using Activity.Common.Security;
using Activity.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Activity.Features.Sessions.GetSessionsToday;

public class GetSessionsTodayHandler(ActivityDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<GetSessionsTodayResponse>>> Handle(
        GetSessionsTodayQuery query, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin && !userAccessor.IsCoach)
            return Error.Unauthorized("Sessions.Unauthorized", "Only Admins and Coaches can view today's sessions.");

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await context.MemberActivities
            .AsNoTracking()
            .Where(a => a.CheckInTime >= today && a.CheckInTime < tomorrow)
            .OrderByDescending(a => a.CheckInTime)
            .Select(a => new GetSessionsTodayResponse(
                a.Id, a.MemberId, $"Member-{a.MemberId.ToString().Substring(0, 8)}",
                a.CardUid, a.CourseId, a.CheckInTime, a.CheckOutTime,
                a.CheckOutTime == null))
            .ToListAsync(ct);
    }
}
