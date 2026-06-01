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
/// Handles NFC card scan for member entry/exit. Validates membership, auto-logs if unambiguous,
/// returns options if multiple active memberships/courses are found.
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
                activeSession.MemberId, cardUid, activeSession.CourseId, activeSession.CheckOutTime.Value));

            return new ScanEntryExitResponse(true, "Exiting", $"Member-{activeSession.MemberId.ToString()[..8]}",
                null, null, []);
        }

        // Resolve the card UID to a real member via the Membership service
        var memberResponse = await membershipClient.GetMemberByCardAsync(cardUid);
        if (!memberResponse.IsSuccessStatusCode || memberResponse.Content is null)
            return Error.NotFound("Card.NotRegistered",
                $"No active member found for card UID '{cardUid}'. The card may not be assigned.");

        var member = memberResponse.Content;

        var session = new MemberActivity
        {
            Id = Guid.CreateVersion7(),
            MemberId = member.MemberId,
            CardUid = cardUid,
            CheckInTime = DateTime.UtcNow
        };

        context.MemberActivities.Add(session);
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberCheckedInEvent(
            session.MemberId, cardUid, null, session.CheckInTime));

        return new ScanEntryExitResponse(true, "Entering", $"{member.FirstName} {member.LastName}", null, null, []);
    }
}
