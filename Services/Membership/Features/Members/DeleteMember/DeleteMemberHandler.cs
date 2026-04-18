using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.DeleteMember;

public class DeleteMemberHandler(
    MembershipDbContext context,
    IIdentityServiceClient identityClient,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<Success>> Handle(DeleteMemberCommand command, CancellationToken ct)
    {
        // 1. Authorization Check (Admin only)
        if (!userAccessor.IsAdmin)
        {
            return Error.Unauthorized("Member.Unauthorized", "Only Administrators can deactivate members.");
        }

        // 2. Retrieve Member
        var member = await context.Members
            .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

        if (member is null)
        {
            return Error.NotFound("Member.NotFound", "The specified member was not found.");
        }

        // 3. Deactivate in Identity Service
        var identityResponse = await identityClient.DeactivateUserAsync(member.UserId);
        if (!identityResponse.IsSuccessStatusCode || identityResponse.Content is null || !identityResponse.Content.Success)
        {
            return Error.Failure("Identity.DeactivationFailed", 
                identityResponse.Error?.Content ?? "Failed to deactivate user in Identity Service.");
        }

        // 4. Soft Delete in Membership Service
        member.Status = MemberStatus.Inactive;
        
        // TODO: Cancel future bookings and active subscriptions here in the future

        await context.SaveChangesAsync(ct);

        return Result.Success;
    }
}
