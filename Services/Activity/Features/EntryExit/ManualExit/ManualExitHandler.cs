using Activity.Common.Security;
using Activity.Domain;
using Activity.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Activity.Features.EntryExit.ManualExit;

public class ManualExitHandler(
    ActivityDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<ManualExitResponse>> Handle(
        ManualExitCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("EntryExit.Unauthorized", "Only Administrators can manually log exit.");

        var activeSession = await context.MemberActivities
            .FirstOrDefaultAsync(a =>
                a.MemberId == command.Request.MemberId && a.CheckOutTime == null, ct);

        if (activeSession is null)
            return Error.NotFound("Session.NotFound", "No active session found for this member.");

        activeSession.CheckOutTime = DateTime.UtcNow;
        activeSession.CourseId ??= command.Request.CourseId;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberCheckedOutEvent(
            activeSession.MemberId, activeSession.CardUid,
            activeSession.CourseId, activeSession.CheckOutTime.Value));

        return new ManualExitResponse(activeSession.Id, activeSession.CheckOutTime.Value);
    }
}
