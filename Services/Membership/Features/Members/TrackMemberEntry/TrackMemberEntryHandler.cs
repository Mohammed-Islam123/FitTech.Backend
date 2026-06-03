using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.TrackMemberEntry;

/// <description>
/// Records a member gym entry. Validates eligibility (active status, active subscription,
/// remaining sessions) and decrements the session count for session-limited plans.
/// If remaining sessions reaches zero, the subscription is automatically expired.
/// </description>
public class TrackMemberEntryHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<TrackMemberEntryResponse>> Handle(
        TrackMemberEntryCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin && !userAccessor.IsCoach)
        {
            return Error.Unauthorized(
                "Member.Unauthorized",
                "Only Administrators and Coaches can record member entry.");
        }

        var member = await context.Members
            .Include(m => m.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(m => m.Id == command.Request.MemberId, ct);

        if (member is null)
        {
            return Error.NotFound(
                "Member.NotFound",
                $"Member with ID {command.Request.MemberId} was not found.");
        }

        if (member.Status != MemberStatus.Active)
        {
            return Error.Forbidden(
                "Member.NotActive",
                $"Member is {member.Status}. Only active members can enter the gym.");
        }

        var activeSubscription = member.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartOnUTC)
            .FirstOrDefault();

        if (activeSubscription is null)
        {
            return Error.Forbidden(
                "Subscription.None",
                "No active subscription found. The member must have an active subscription to enter.");
        }

        // Check remaining sessions for session-limited plans
        if (activeSubscription.RemainingSessions is not null)
        {
            if (activeSubscription.RemainingSessions <= 0)
            {
                return Error.Forbidden(
                    "Subscription.NoSessions",
                    "All sessions have been used. The subscription has no remaining sessions.");
            }

            activeSubscription.RemainingSessions--;

            if (activeSubscription.RemainingSessions == 0)
            {
                activeSubscription.Status = SubscriptionStatus.Expired;
            }
        }

        await context.SaveChangesAsync(ct);

        return new TrackMemberEntryResponse(
            true,
            $"{member.FirstName} {member.LastName}",
            activeSubscription.RemainingSessions,
            new ActiveMembershipInfo(
                activeSubscription.Id,
                activeSubscription.Plan.Name,
                activeSubscription.EndOnUTC,
                activeSubscription.RemainingSessions,
                activeSubscription.Status.ToString()));
    }
}
