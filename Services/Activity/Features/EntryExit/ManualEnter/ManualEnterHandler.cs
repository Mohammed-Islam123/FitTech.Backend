using Activity.Common.Security;
using Activity.Domain;
using Activity.Domain.Entities;
using ErrorOr;
using Shared.Events;
using Wolverine;

namespace Activity.Features.EntryExit.ManualEnter;

public class ManualEnterHandler(
    ActivityDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<ManualEnterResponse>> Handle(
        ManualEnterCommand command, CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("EntryExit.Unauthorized", "Only Administrators can manually log entry.");

        var session = new MemberActivity
        {
            Id = Guid.CreateVersion7(),
            MemberId = command.Request.MemberId,
            CourseId = command.Request.CourseId,
            CheckInTime = DateTime.UtcNow,
            IsManual = true
        };

        context.MemberActivities.Add(session);
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberCheckedInEvent(
            session.MemberId, null, session.CourseId, session.CheckInTime));

        return new ManualEnterResponse(session.Id, session.CheckInTime);
    }
}
