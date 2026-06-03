using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Subscriptions.RequestPurchase;

/// <description>
/// Member submits a cash purchase request for a new plan. Validates member status,
/// plan existence, and that no active subscription exists. Creates a
/// PaymentApprovalRequest with status Pending for admin review.
/// </description>
public class RequestPurchaseHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<RequestPurchaseResponse>> Handle(
        RequestPurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Request.Unauthorized", "Only Members can submit purchase requests.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Request.Unauthorized", "Authentication required.");

        var req = command.Request;

        // Validate member exists and is active
        var member = await context.Members
            .FirstOrDefaultAsync(m => m.UserId == userId.Value, ct);

        if (member is null || member.Status != MemberStatus.Active)
            return Error.NotFound("Member.NotFound", "Member profile not found or is inactive.");

        // Validate plan exists and is active
        var plan = await context.SubscriptionPlans
            .FindAsync([req.PlanId], cancellationToken: ct);

        if (plan is null || !plan.IsActive)
            return Error.NotFound("Plan.NotFound", "Plan does not exist or is inactive.");

        // Block if member already has an active subscription
        var existingSubscription = await context.Subscriptions
            .Where(s => s.MemberId == member.Id && s.Status == SubscriptionStatus.Active)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (existingSubscription is not null)
            return Error.Conflict("Subscription.AlreadyActive",
                "You already have an active subscription. Wait for it to expire or contact the gym to switch plans.");

        // Create the approval request — ReferenceId stores PlanId
        var request = new PaymentApprovalRequest
        {
            Id = Guid.CreateVersion7(),
            MemberId = userId.Value,
            RequestType = PaymentApprovalRequestType.PlanPurchase,
            Amount = plan.Price,
            ReferenceId = req.PlanId,
            Notes = req.Notes,
            Status = PaymentApprovalRequestStatus.Pending
        };

        context.PaymentApprovalRequests.Add(request);
        await context.SaveChangesAsync(ct);

        return new RequestPurchaseResponse(request.Id);
    }
}
