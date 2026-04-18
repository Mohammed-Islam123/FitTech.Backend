using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Members.GetSubscriptionHistory;

public class GetSubscriptionHistoryHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<SubscriptionHistoryResponse>> Handle(
        GetSubscriptionHistoryQuery query,
        CancellationToken ct)
    {
        var memberInfo = await context.Members
            .AsNoTracking()
            .Where(m => m.Id == query.MemberId)
            .Select(m => new { m.UserId })
            .FirstOrDefaultAsync(ct);

        if (memberInfo == null)
        {
            return Error.NotFound("Member.NotFound", $"Member with ID {query.MemberId} was not found.");
        }

        // Authorization: Admin/Coach can see anyone. Member can only see themselves.
        if (!userAccessor.IsAdmin && !userAccessor.IsCoach)
        {
            if (userAccessor.UserId != memberInfo.UserId)
            {
                return Error.Forbidden("Member.Forbidden", "You are not authorized to view this member's subscription history.");
            }
        }

        var subscriptions = await context.Subscriptions
            .AsNoTracking()
            .Where(s => s.MemberId == query.MemberId)
            .OrderByDescending(s => s.StartOnUTC)
            .Select(s => new SubscriptionHistoryItem(
                s.Id,
                s.Plan.Name,
                s.StartOnUTC,
                s.EndOnUTC,
                s.Plan.Price,
                s.Status,
                s.RemainingSessions,
                s.CancelledAt
            ))
            .ToListAsync(ct);

        return new SubscriptionHistoryResponse(subscriptions);
    }
}
