using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Me.Sessions;

/// <description>
/// Returns the authenticated member's gym entry/exit session history by proxying
/// to the Activity service. Resolves UserId (JWT) to MemberId (Membership internal)
/// before calling Activity.
/// </description>
public class MeSessionsHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IActivityServiceClient activityClient)
{
    public async Task<ErrorOr<List<MeSessionResponse>>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Me.Unauthorized", "Only Members can view their session history.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Me.Unauthorized", "Authentication required.");

        var member = await context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId.Value, ct);

        if (member is null)
            return Error.NotFound("Member.NotFound", "Member profile not found.");

        var response = await activityClient.GetMemberActivityAsync(member.Id);
        if (!response.IsSuccessStatusCode || response.Content is null)
            return Error.Failure("Activity.Failed", "Failed to retrieve session history from Activity service.");

        return response.Content.Select(s => new MeSessionResponse(
            s.SessionId,
            s.CheckInTime,
            s.CheckOutTime,
            s.CourseId,
            s.IsManual
        )).ToList();
    }
}
