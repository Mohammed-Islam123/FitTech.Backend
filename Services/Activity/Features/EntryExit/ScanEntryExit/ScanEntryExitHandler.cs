using Activity.Common.Security;
using Activity.Domain;
using Activity.Domain.Entities;
using Activity.Infrastructure;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Activity.Features.EntryExit.ScanEntryExit;

/// <description>
/// Handles NFC card scan for member entry/exit. Validates member eligibility (active status,
/// active subscription, remaining sessions), auto-logs entry or exit, and tracks session usage.
/// </description>
public class ScanEntryExitHandler(
    ActivityDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus,
    IMembershipServiceClient membershipClient)
{
    public async Task<ErrorOr<ScanEntryExitResponse>> Handle(
        ScanEntryExitCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("EntryExit.Unauthorized", "Only Administrators can scan entry/exit.");

        var cardUid = command.Request.CardUid;

        var activeSession = await context.MemberActivities
            .FirstOrDefaultAsync(a => a.CardUid == cardUid && a.CheckOutTime == null, ct);

        if (activeSession is not null)
        {
            activeSession.CheckOutTime = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);

            await messageBus.PublishAsync(new MemberCheckedOutEvent(
                activeSession.MemberId, cardUid, null, activeSession.CheckOutTime.Value));

            return new ScanEntryExitResponse(true, "Exiting", activeSession.MemberName,
                null, null, []);
        }

        // Resolve the card UID to a real member via the Membership service
        var memberResponse = await membershipClient.GetMemberByCardAsync(cardUid);
        if (!memberResponse.IsSuccessStatusCode || memberResponse.Content is null)
            return Error.NotFound("Card.NotRegistered",
                $"No active member found for card UID '{cardUid}'. The card may not be assigned.");

        var member = memberResponse.Content;

        // Eligibility check: member status
        if (member.Status != "Active")
            return Error.Forbidden("Member.NotActive",
                $"Member is {member.Status}. Only active members can enter.");

        // Eligibility check: active subscription
        if (member.ActiveSubscription is null)
            return Error.Forbidden("Subscription.None",
                "No active subscription found. The member must have an active subscription to enter.");

        if (member.ActiveSubscription.Status != "Active")
            return Error.Forbidden("Subscription.NotActive",
                $"Subscription is {member.ActiveSubscription.Status}.");

        // Eligibility check: remaining sessions
        if (member.ActiveSubscription.RemainingSessions is not null && member.ActiveSubscription.RemainingSessions <= 0)
            return Error.Forbidden("Subscription.NoSessions",
                "All sessions have been used. The subscription has no remaining sessions.");

        // Track entry in the Membership service (decrements sessions, auto-expires at 0)
        var trackResponse = await membershipClient.TrackMemberEntryAsync(
            new TrackEntryRequest(member.MemberId));

        var remainingSessions = member.ActiveSubscription.RemainingSessions;
        ActiveMembershipInfo? activeMembership = null;

        if (trackResponse.IsSuccessStatusCode && trackResponse.Content is not null)
        {
            remainingSessions = trackResponse.Content.RemainingSessions;
        if (trackResponse.Content.ActiveSubscription is not null)
        {
            activeMembership = new ActiveMembershipInfo(
                trackResponse.Content.ActiveSubscription.SubscriptionId,
                trackResponse.Content.ActiveSubscription.PlanName,
                trackResponse.Content.ActiveSubscription.EndOnUTC);
        }
        }

        var memberName = $"{member.FirstName} {member.LastName}";

        var session = new MemberActivity
        {
            Id = Guid.CreateVersion7(),
            MemberId = member.MemberId,
            MemberName = memberName,
            CardUid = cardUid,
            CheckInTime = DateTime.UtcNow
        };

        context.MemberActivities.Add(session);
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberCheckedInEvent(
            session.MemberId, cardUid, null, session.CheckInTime));

        return new ScanEntryExitResponse(true, "Entering", memberName,
            remainingSessions, activeMembership, []);
    }
}
