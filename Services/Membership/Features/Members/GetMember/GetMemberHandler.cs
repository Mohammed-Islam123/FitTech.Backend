using ErrorOr;
using Membership.Domain;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.GetMember;

public class GetMemberHandler(MembershipDbContext context)
{
    public async Task<ErrorOr<GetMemberResponse>> Handle(
        GetMemberQuery query,
        CancellationToken ct)
    {
        var response = await context.Members
            .AsNoTracking()
            .Where(m => m.Id == query.MemberId)
            .Select(m => new GetMemberResponse(
                m.Id,
                m.UserId,
                m.FirstName,
                m.LastName,
                m.JoinDate,
                m.Status,
                m.NoShowWarningCount,
                m.Subscriptions
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
                m.HealthProfile != null ? new HealthProfileResponse(
                    m.HealthProfile.Objectives,
                    m.HealthProfile.MedicalRestrictions,
                    m.HealthProfile.LastUpdatedAt) : null,
                m.NfcCards
                    .Where(c => c.IsActive)
                    .Select(c => c.CardUid)
                    .FirstOrDefault()
            ))
            .FirstOrDefaultAsync(ct);

        return response is null ? (ErrorOr<GetMemberResponse>)Error.NotFound("Member.NotFound", $"Member with ID {query.MemberId} was not found.") : (ErrorOr<GetMemberResponse>)response;
    }
}
