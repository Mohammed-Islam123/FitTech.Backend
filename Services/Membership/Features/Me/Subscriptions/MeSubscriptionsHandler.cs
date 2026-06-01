using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Me.Subscriptions;

/// <description>
/// Returns the authenticated member's subscription history using JWT UserId to
/// resolve Membership's internal MemberId, then querying subscriptions directly.
/// </description>
public class MeSubscriptionsHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<MeSubscriptionResponse>>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Me.Unauthorized", "Only Members can view their subscription history.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Me.Unauthorized", "Authentication required.");

        var member = await context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId.Value, ct);

        if (member is null)
            return Error.NotFound("Member.NotFound", "Member profile not found.");

        return await context.Subscriptions
            .AsNoTracking()
            .Where(s => s.MemberId == member.Id)
            .OrderByDescending(s => s.StartOnUTC)
            .Select(s => new MeSubscriptionResponse(
                s.Id,
                s.Plan.Name,
                s.Plan.Price,
                s.StartOnUTC,
                s.EndOnUTC,
                s.RemainingSessions,
                s.Status,
                s.PaymentStatus.ToString(),
                s.AutoRenew
            ))
            .ToListAsync(ct);
    }
}
