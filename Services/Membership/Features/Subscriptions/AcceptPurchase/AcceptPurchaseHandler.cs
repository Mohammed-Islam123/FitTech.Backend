using ErrorOr;
using Membership.Common.Security;
using Membership.Domain;
using Membership.Domain.Entities;
using Membership.Domain.Enums;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Events;
using Wolverine;

namespace Membership.Features.Subscriptions.AcceptPurchase;

/// <description>
/// Admin accepts a cash purchase request. Creates the subscription, records the payment
/// in the Payment service, and publishes a thank-you email.
/// </description>
public class AcceptPurchaseHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<AcceptPurchaseResponse>> Handle(
        AcceptPurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can accept requests.");

        var req = command.Request;

        var approvalRequest = await context.PaymentApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == req.RequestId, ct);

        if (approvalRequest is null)
            return Error.NotFound("Request.NotFound", "Purchase request not found.");

        if (approvalRequest.RequestType != PaymentApprovalRequestType.PlanPurchase)
            return Error.Validation("Request.InvalidType", "This request is not a plan purchase request.");

        if (approvalRequest.Status != PaymentApprovalRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "This request has already been resolved.");

        // Get the plan — ReferenceId stores the PlanId
        var plan = await context.SubscriptionPlans
            .FindAsync([approvalRequest.ReferenceId], cancellationToken: ct);

        if (plan is null || !plan.IsActive)
            return Error.NotFound("Plan.NotFound", "The plan for this purchase is no longer available.");

        // Get the member
        var member = await context.Members
            .FirstOrDefaultAsync(m => m.UserId == approvalRequest.MemberId, ct);

        if (member is null || member.Status != MemberStatus.Active)
            return Error.NotFound("Member.NotFound", "The member is no longer active.");

        // Block if member already has an active subscription (double-check at accept time)
        var existingSubscription = await context.Subscriptions
            .Where(s => s.MemberId == member.Id && s.Status == SubscriptionStatus.Active)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (existingSubscription is not null)
            return Error.Conflict("Subscription.AlreadyActive",
                "The member already has an active subscription.");

        // Step 1: Create the subscription
        var startOnUtc = DateTime.UtcNow;
        DateTime? endOnUtc = null;

        if (plan.DurationUnit == DurationUnit.Months && plan.DurationValue.HasValue)
        {
            endOnUtc = startOnUtc.AddMonths(plan.DurationValue.Value);
        }
        else if (plan.DurationUnit == DurationUnit.Days && plan.DurationValue.HasValue)
        {
            endOnUtc = startOnUtc.AddDays(plan.DurationValue.Value);
        }

        var subscription = new Subscription
        {
            Id = Guid.CreateVersion7(),
            MemberId = member.Id,
            PlanId = plan.Id,
            StartOnUTC = startOnUtc,
            EndOnUTC = endOnUtc,
            RemainingSessions = plan.SessionCount,
            Status = SubscriptionStatus.Active,
            AutoRenew = false,
            PaymentId = null,
            PaymentStatus = PaymentStatus.Pending
        };

        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync(ct);

        // Step 2: Create payment record in Payment service
        var paymentPayload = new CreatePaymentRequest(
            UserId: member.UserId,
            Amount: approvalRequest.Amount,
            PaymentMethod: PaymentMethod.Cash.ToString(),
            PaymentType: PaymentType.Subscription.ToString(),
            ReferenceId: subscription.Id,
            Notes: req.Notes ?? approvalRequest.Notes);

        var paymentResponse = await paymentClient.CreatePaymentAsync(paymentPayload);

        if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
            return Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.");

        // Step 3: Mark everything as complete
        approvalRequest.Status = PaymentApprovalRequestStatus.Accepted;
        approvalRequest.ResolvedAt = DateTime.UtcNow;

        subscription.PaymentId = paymentResponse.Content.PaymentId;
        subscription.PaymentStatus = PaymentStatus.Paid;

        await context.SaveChangesAsync(ct);

        // Step 4: Publish thank-you email
        var endDateText = endOnUtc?.ToString("yyyy-MM-dd") ?? "N/A (session-based)";

        await messageBus.PublishAsync(new SendEmailEvent(
            To: member.FirstName,
            Subject: $"Subscription Activated - {plan.Name}",
            Body: $"<h2>Thank You for Your Purchase!</h2>" +
                  $"<p>Dear {member.FirstName},</p>" +
                  $"<p>Your subscription has been activated after cash payment confirmation.</p>" +
                  $"<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;'>" +
                  $"<tr><td><strong>Plan</strong></td><td>{plan.Name}</td></tr>" +
                  $"<tr><td><strong>Amount Paid</strong></td><td>{approvalRequest.Amount} DZD</td></tr>" +
                  $"<tr><td><strong>Payment Method</strong></td><td>Cash</td></tr>" +
                  $"<tr><td><strong>Start Date</strong></td><td>{DateTime.UtcNow:yyyy-MM-dd}</td></tr>" +
                  $"<tr><td><strong>Valid Until</strong></td><td>{endDateText}</td></tr>" +
                  $"</table>" +
                  $"<p>Thank you for choosing FitTech! Keep pushing your limits.</p>"
        ));

        return new AcceptPurchaseResponse(approvalRequest.Id, paymentResponse.Content.PaymentId, approvalRequest.Status);
    }
}
