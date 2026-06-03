using Activity.Common.Security;
using Activity.Domain;
using Activity.Domain.Entities;
using Activity.Infrastructure;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Activity.Features.EntryExit.ManualEnter;

public class ManualEnterHandler(
    ActivityDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus,
    IMembershipServiceClient membershipClient)
{
    public async Task<ErrorOr<ManualEnterResponse>> Handle(
        ManualEnterCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("EntryExit.Unauthorized", "Only Administrators can manually log entry.");

        // Duplicate-entry guard: prevent creating a second active session
        var existingSession = await context.MemberActivities
            .FirstOrDefaultAsync(a =>
                a.MemberId == command.Request.MemberId && a.CheckOutTime == null, ct);

        if (existingSession is not null)
            return Error.Conflict("EntryExit.AlreadyCheckedIn",
                "This member already has an active session. Check them out first before creating a new entry.");

        // Validate eligibility and track entry via Membership service
        var trackResponse = await membershipClient.TrackMemberEntryAsync(
            new TrackEntryRequest(command.Request.MemberId));

        if (!trackResponse.IsSuccessStatusCode || trackResponse.Content is null)
            return Error.Forbidden("EntryExit.EligibilityFailed",
                "Member eligibility check failed. Ensure the member is active and has a valid subscription.");

        var memberName = trackResponse.Content.MemberName;

        var session = new MemberActivity
        {
            Id = Guid.CreateVersion7(),
            MemberId = command.Request.MemberId,
            MemberName = memberName,
            CheckInTime = DateTime.UtcNow,
            IsManual = true
        };

        context.MemberActivities.Add(session);
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberCheckedInEvent(
            session.MemberId, null, null, session.CheckInTime));

        return new ManualEnterResponse(session.Id, session.CheckInTime);
    }
}
