using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Membership.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membership.Features.Subscriptions.RequestRenewal;

/// <description>
/// Member submits a cash renewal request. Validates ownership, expired status,
/// and amount against plan price. Creates a PaymentApprovalRequest with status
/// Pending for admin review.
/// </description>
public class RequestRenewalHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor)
{
    public async Task<ErrorOr<RequestRenewalResponse>> Handle(
        RequestRenewalCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Request.Unauthorized", "Only Members can submit renewal requests.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Request.Unauthorized", "Authentication required.");

        var req = command.Request;

        // Validate subscription ownership, existence, and status
        var subscription = await context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == req.SubscriptionId, ct);

        if (subscription is null)
            return Error.NotFound("Subscription.NotFound", "The specified subscription does not exist.");

        if (subscription.MemberId != userId.Value)
            return Error.Forbidden("Subscription.Forbidden", "This subscription does not belong to you.");

        if (subscription.Status != SubscriptionStatus.Expired)
            return Error.Validation("Subscription.NotExpired",
                "You can only renew an expired subscription. To switch plans, purchase a new subscription instead.");

        // Validate amount against plan price
        if (req.Amount != subscription.Plan.Price)
            return Error.Validation("Payment.AmountMismatch",
                $"The renewal amount must match the plan price of {subscription.Plan.Price} DZD.");

        var request = new PaymentApprovalRequest
        {
            Id = Guid.CreateVersion7(),
            MemberId = userId.Value,
            RequestType = PaymentApprovalRequestType.MembershipRenewal,
            Amount = req.Amount,
            ReferenceId = req.SubscriptionId,
            Notes = req.Notes,
            Status = PaymentApprovalRequestStatus.Pending
        };

        context.PaymentApprovalRequests.Add(request);
        await context.SaveChangesAsync(ct);

        return new RequestRenewalResponse(request.Id);
    }
}
