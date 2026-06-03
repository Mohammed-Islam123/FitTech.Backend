using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Subscriptions.ListPurchaseRequests;

/// <description>
/// Returns all pending cash plan purchase requests for admin review.
/// Admin-only access.
/// </description>
public class ListPurchaseRequestsHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<List<ListPurchaseRequestsResponse>>> Handle(
        ListPurchaseRequestsQuery query,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can view purchase requests.");

        var requests = await context.PaymentApprovalRequests
            .AsNoTracking()
            .Where(r => r.RequestType == PaymentApprovalRequestType.PlanPurchase
                        && r.Status == PaymentApprovalRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        // Enrich with member and plan info
        var planIds = requests.Select(r => r.ReferenceId).Distinct().ToList();
        var plans = await context.SubscriptionPlans
            .AsNoTracking()
            .Where(p => planIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var userIds = requests.Select(r => r.MemberId).Distinct().ToList();
        var members = await context.Members
            .AsNoTracking()
            .Where(m => userIds.Contains(m.UserId))
            .ToDictionaryAsync(m => m.UserId, ct);

        return requests.Select(r =>
        {
            var plan = plans.GetValueOrDefault(r.ReferenceId);
            var member = members.GetValueOrDefault(r.MemberId);
            return new ListPurchaseRequestsResponse(
                RequestId: r.Id,
                MemberId: r.MemberId,
                MemberName: member?.FirstName + " " + member?.LastName ?? "Unknown",
                PlanId: r.ReferenceId,
                PlanName: plan?.Name ?? "Unknown Plan",
                Amount: r.Amount,
                Status: r.Status,
                CreatedAt: r.CreatedAt,
                Notes: r.Notes
            );
        }).ToList();
    }
}
