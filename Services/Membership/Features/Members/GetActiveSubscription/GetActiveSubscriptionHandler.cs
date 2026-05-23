using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.GetActiveSubscription;

/// <description>
/// Handles the retrieval of the current active subscription for a member.
/// </description>
public class GetActiveSubscriptionHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetActiveSubscriptionResponse>> Handle(
        GetActiveSubscriptionQuery query,
        CancellationToken ct)
    {
        Guid targetMemberId;

        if (query.MemberId.HasValue)
        {
            if (!userAccessor.IsAdmin)
            {
                return Error.Forbidden(
                    "Member.Forbidden", 
                    "Only administrators can request active subscriptions for other members.");
            }
            targetMemberId = query.MemberId.Value;
        }
        else
        {
            // Resolve MemberId from the authenticated user's Identity ID
            var memberId = await context.Members
                .AsNoTracking()
                .Where(m => m.UserId == userAccessor.UserId)
                .Select(m => m.Id)
                .FirstOrDefaultAsync(ct);

            if (memberId == Guid.Empty)
            {
                return Error.NotFound(
                    "Member.NotFound", 
                    "No member record found for the current user.");
            }
            targetMemberId = memberId;
        }

        var now = DateTime.UtcNow;

        var activeSubscription = await context.Subscriptions
            .AsNoTracking()
            .Where(s => s.MemberId == targetMemberId &&
                        s.Status == SubscriptionStatus.Active &&
                        s.StartOnUTC <= now &&
                        (s.EndOnUTC == null || s.EndOnUTC >= now))
            .Select(s => new GetActiveSubscriptionResponse(
                s.Id,
                s.Plan.Name,
                s.Plan.Price,
                s.StartOnUTC,
                s.EndOnUTC,
                s.Status,
                s.RemainingSessions,
                s.PausedUntil))
            .FirstOrDefaultAsync(ct);

        if (activeSubscription is null)
        {
            return Error.NotFound(
                "Membership.ActiveSubscriptionNotFound", 
                "No active subscription found for this member.");
        }

        return activeSubscription;
    }
}
