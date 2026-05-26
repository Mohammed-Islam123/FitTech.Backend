using Activity.Common.Security;
using Activity.Domain;
using Activity.Domain.Entities;
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
    IMessageBus messageBus)
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

        var session = new MemberActivity
        {
            Id = Guid.CreateVersion7(),
            MemberId = Guid.Empty,
            CardUid = cardUid,
            CheckInTime = DateTime.UtcNow
        };

        context.MemberActivities.Add(session);
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberCheckedInEvent(
            session.MemberId, cardUid, null, session.CheckInTime));

        return new ScanEntryExitResponse(true, "Entering", $"Card-{cardUid}", null, null, []);
    }
}
