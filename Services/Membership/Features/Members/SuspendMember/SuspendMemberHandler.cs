using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Membership.Features.Members.SuspendMember;

/// <description>
/// Suspends a member account. Sets status to Suspended and publishes a MemberSuspendedEvent.
/// </description>
public class SuspendMemberHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<SuspendMemberResponse>> Handle(
        SuspendMemberCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Member.Unauthorized",
                "Only Administrators can suspend member accounts.");
        }

        var member = await context.Members
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, ct);

        if (member is null)
        {
            return Error.NotFound(
                "Member.NotFound",
                $"Member with ID {command.MemberId} was not found.");
        }

        if (member.Status == MemberStatus.Suspended)
        {
            return Error.Conflict(
                "Member.AlreadySuspended",
                "This member is already suspended.");
        }

        member.Status = MemberStatus.Suspended;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberSuspendedEvent(
            MemberId: member.Id,
            UserId: member.UserId,
            MemberFullName: $"{member.FirstName} {member.LastName}",
            Email: string.Empty,
            SuspendedAt: DateTime.UtcNow));

        return new SuspendMemberResponse(member.Id, member.Status.ToString());
    }
}
