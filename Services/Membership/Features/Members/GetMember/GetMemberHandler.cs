using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.GetMember;

public class GetMemberHandler(MembershipDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<GetMemberResponse>> Handle(
        GetMemberQuery query,
        CancellationToken ct)
    {
        var currentUserId = userAccessor.UserId;
        var isCoach = userAccessor.IsCoach;
        var isAdmin = userAccessor.IsAdmin;

        var member = await context.Members
            .AsNoTracking()
            .Include(m => m.HealthProfile)
            .Include(m => m.Subscriptions).ThenInclude(s => s.Plan)
            .Include(m => m.NfcCards)
            .FirstOrDefaultAsync(m => m.Id == query.MemberId, ct);

        if (member is null)
        {
            return Error.NotFound("Member.NotFound", $"Member with ID {query.MemberId} was not found.");
        }

        // Logic for role-based health profile visibility
        HealthProfileResponse? healthProfileResponse = null;
        if (member.HealthProfile != null)
        {
            bool isOwner = member.UserId == currentUserId;
            
            if (isOwner || isCoach)
            {
                // Full access
                healthProfileResponse = new HealthProfileResponse(
                    member.HealthProfile.Objectives,
                    member.HealthProfile.MedicalRestrictions,
                    member.HealthProfile.LastUpdatedAt);
            }
            else if (isAdmin)
            {
                // Restricted access (Admin only sees medical restrictions)
                healthProfileResponse = new HealthProfileResponse(
                    null, // Objectives are private
                    member.HealthProfile.MedicalRestrictions,
                    member.HealthProfile.LastUpdatedAt);
            }
        }

        var response = new GetMemberResponse(
            member.Id,
            member.UserId,
            member.FirstName,
            member.LastName,
            member.JoinDate,
            member.Status,
            member.NoShowWarningCount,
            member.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartOnUTC)
                .Select(s => new ActiveSubscriptionResponse(
                    s.Id,
                    s.Plan.Name,
                    s.StartOnUTC,
                    s.EndOnUTC,
                    s.RemainingSessions,
                    s.Status))
                .FirstOrDefault(),
            healthProfileResponse,
            member.NfcCards
                .Where(c => c.IsActive)
                .Select(c => c.CardUid)
                .FirstOrDefault()
        );

        return response;
    }
}
