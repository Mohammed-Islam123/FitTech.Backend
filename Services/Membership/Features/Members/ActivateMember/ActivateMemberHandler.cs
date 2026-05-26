using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Wolverine;

namespace Membership.Features.Members.ActivateMember;

/// <description>
/// Activates a suspended member account. Sets status to Active and publishes a MemberActivatedEvent.
/// </description>
public class ActivateMemberHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<ActivateMemberResponse>> Handle(
        ActivateMemberCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized(
                "Member.Unauthorized",
                "Only Administrators can activate member accounts.");
        }

        var member = await context.Members
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, ct);

        if (member is null)
        {
            return Error.NotFound(
                "Member.NotFound",
                $"Member with ID {command.MemberId} was not found.");
        }

        if (member.Status == MemberStatus.Active)
        {
            return Error.Conflict(
                "Member.AlreadyActive",
                "This member is already active.");
        }

        member.Status = MemberStatus.Active;
        await context.SaveChangesAsync(ct);

        await messageBus.PublishAsync(new MemberActivatedEvent(
            MemberId: member.Id,
            UserId: member.UserId,
            MemberFullName: $"{member.FirstName} {member.LastName}",
            Email: string.Empty,
            ActivatedAt: DateTime.UtcNow));

        return new ActivateMemberResponse(member.Id, member.Status.ToString());
    }
}
