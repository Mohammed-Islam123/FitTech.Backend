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

namespace Membership.Features.Subscriptions.OnlineRenewal;

/// <description>
/// Member submits an online (credit card) renewal. No admin approval needed —
/// payment is processed immediately (simulated for now) and the subscription
/// is extended automatically. Publishes a thank-you email with transaction details.
///
/// When a real payment gateway is integrated, replace the manual ConfirmPayment
/// call with the gateway's intent → webhook flow.
/// </description>
public class OnlineRenewalHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<OnlineRenewalResponse>> Handle(
        OnlineRenewalCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Renewal.Unauthorized", "Only Members can renew subscriptions.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Renewal.Unauthorized", "Authentication required.");

        var req = command.Request;

        // Validate subscription ownership, existence, and status
        var subscription = await context.Subscriptions
            .Include(s => s.Member)
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

        // Step 1: Create payment record in Payment service (CreditCard → Pending)
        var paymentPayload = new CreatePaymentRequest(
            UserId: subscription.Member.UserId,
            Amount: req.Amount,
            PaymentMethod: PaymentMethod.CreditCard.ToString(),
            PaymentType: PaymentType.Subscription.ToString(),
            ReferenceId: subscription.Id,
            Notes: req.Notes);

        var paymentResponse = await paymentClient.CreatePaymentAsync(paymentPayload);

        if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
            return Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.");

        var paymentId = paymentResponse.Content.PaymentId;

        // Step 2: Confirm the payment (simulated — replaces real gateway + webhook flow)
        var confirmResponse = await paymentClient.ConfirmPaymentAsync(paymentId);

        if (!confirmResponse.IsSuccessStatusCode)
            return Error.Failure("Payment.ConfirmFailed", "Failed to confirm payment.");

        // Step 3: Create the renewed subscription
        var newStart = subscription.EndOnUTC ?? DateTime.UtcNow;
        var newEnd = subscription.Plan.DurationValue.HasValue
            ? subscription.Plan.DurationUnit switch
            {
                DurationUnit.Days => newStart.AddDays(subscription.Plan.DurationValue.Value),
                _ => newStart.AddMonths(subscription.Plan.DurationValue ?? 1)
            }
            : (DateTime?)null;

        var renewedSubscription = new Subscription
        {
            Id = Guid.CreateVersion7(),
            MemberId = subscription.MemberId,
            PlanId = subscription.PlanId,
            StartOnUTC = newStart,
            EndOnUTC = newEnd,
            RemainingSessions = subscription.Plan.SessionCount,
            Status = SubscriptionStatus.Active,
            PaymentId = paymentId,
            PaymentStatus = PaymentStatus.Paid,
            AutoRenew = subscription.AutoRenew
        };

        context.Subscriptions.Add(renewedSubscription);
        subscription.Status = SubscriptionStatus.Expired;

        await context.SaveChangesAsync(ct);

        // Step 4: Publish thank-you email
        var member = subscription.Member;
        var plan = subscription.Plan;
        var endDateText = newEnd?.ToString("yyyy-MM-dd") ?? "N/A (session-based)";

        await messageBus.PublishAsync(new SendEmailEvent(
            To: member.FirstName,
            Subject: $"Renewal Confirmed - {plan.Name}",
            Body: $"<h2>Thank You for Your Renewal!</h2>" +
                  $"<p>Dear {member.FirstName},</p>" +
                  $"<p>Your online membership renewal has been confirmed and your subscription is now active.</p>" +
                  $"<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;'>" +
                  $"<tr><td><strong>Plan</strong></td><td>{plan.Name}</td></tr>" +
                  $"<tr><td><strong>Amount Paid</strong></td><td>{req.Amount} DZD</td></tr>" +
                  $"<tr><td><strong>Payment Method</strong></td><td>Credit Card (Online)</td></tr>" +
                  $"<tr><td><strong>Renewal Date</strong></td><td>{DateTime.UtcNow:yyyy-MM-dd}</td></tr>" +
                  $"<tr><td><strong>Valid Until</strong></td><td>{endDateText}</td></tr>" +
                  $"</table>" +
                  $"<p>Thank you for choosing FitTech! Keep pushing your limits.</p>"
        ));

        return new OnlineRenewalResponse(
            SubscriptionId: renewedSubscription.Id,
            PaymentId: paymentId,
            PlanName: plan.Name,
            Amount: req.Amount,
            PaymentMethod: PaymentMethod.CreditCard.ToString(),
            RenewedAt: DateTime.UtcNow,
            ValidUntil: newEnd
        );
    }
}
