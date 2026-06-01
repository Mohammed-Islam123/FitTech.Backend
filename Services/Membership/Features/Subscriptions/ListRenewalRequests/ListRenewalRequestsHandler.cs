using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Subscriptions.ListRenewalRequests;

/// <description>
/// Returns all pending cash membership renewal requests for admin review.
/// Admin-only access.
/// </description>
public class ListRenewalRequestsHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListRenewalRequestsResponse>>> Handle(
        ListRenewalRequestsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can view renewal requests.");

        var requests = await context.PaymentApprovalRequests
            .AsNoTracking()
            .Where(r => r.RequestType == PaymentApprovalRequestType.MembershipRenewal
                        && r.Status == PaymentApprovalRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        // Enrich with member and plan info
        var subscriptionIds = requests.Select(r => r.ReferenceId).Distinct().ToList();
        var subscriptions = await context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .Where(s => subscriptionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, ct);

        return requests.Select(r =>
        {
            var sub = subscriptions.GetValueOrDefault(r.ReferenceId);
            return new ListRenewalRequestsResponse(
                RequestId: r.Id,
                MemberId: r.MemberId,
                MemberName: sub?.Member.FirstName + " " + sub?.Member.LastName ?? "Unknown",
                SubscriptionId: r.ReferenceId,
                PlanName: sub?.Plan.Name ?? "Unknown Plan",
                Amount: r.Amount,
                Status: r.Status,
                CreatedAt: r.CreatedAt,
                Notes: r.Notes
            );
        }).ToList();
    }
}
