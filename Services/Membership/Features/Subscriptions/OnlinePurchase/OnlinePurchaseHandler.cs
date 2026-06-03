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

namespace Membership.Features.Subscriptions.OnlinePurchase;

/// <description>
/// Member purchases a new plan online (credit card). No admin approval needed —
/// payment is processed immediately (simulated) and the subscription
/// is activated automatically. Publishes a thank-you email with transaction details.
///
/// When a real payment gateway is integrated, replace the manual ConfirmPayment
/// call with the gateway's intent → webhook flow.
/// </description>
public class OnlinePurchaseHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<OnlinePurchaseResponse>> Handle(
        OnlinePurchaseCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsMember)
            return Error.Unauthorized("Purchase.Unauthorized", "Only Members can purchase subscriptions.");

        var userId = userAccessor.UserId;
        if (userId is null)
            return Error.Unauthorized("Purchase.Unauthorized", "Authentication required.");

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

        // Step 1: Create the subscription first (to get an ID for payment reference)
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

        // Step 2: Create payment record in Payment service (CreditCard → Pending)
        var paymentPayload = new CreatePaymentRequest(
            UserId: member.UserId,
            Amount: plan.Price,
            PaymentMethod: PaymentMethod.CreditCard.ToString(),
            PaymentType: PaymentType.Subscription.ToString(),
            ReferenceId: subscription.Id,
            Notes: req.Notes);

        var paymentResponse = await paymentClient.CreatePaymentAsync(paymentPayload);

        if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
        {
            context.Subscriptions.Remove(subscription);
            await context.SaveChangesAsync(ct);
            return Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.");
        }

        var paymentId = paymentResponse.Content.PaymentId;

        // Step 3: Confirm the payment (simulated — replaces real gateway + webhook flow)
        var confirmResponse = await paymentClient.ConfirmPaymentAsync(paymentId);

        if (!confirmResponse.IsSuccessStatusCode)
        {
            context.Subscriptions.Remove(subscription);
            await context.SaveChangesAsync(ct);
            return Error.Failure("Payment.ConfirmFailed", "Failed to confirm payment.");
        }

        // Step 4: Mark subscription as paid
        subscription.PaymentId = paymentId;
        subscription.PaymentStatus = PaymentStatus.Paid;
        await context.SaveChangesAsync(ct);

        // Step 5: Publish thank-you email
        var endDateText = endOnUtc?.ToString("yyyy-MM-dd") ?? "N/A (session-based)";

        await messageBus.PublishAsync(new SendEmailEvent(
            To: member.FirstName,
            Subject: $"Subscription Activated - {plan.Name}",
            Body: $"<h2>Thank You for Your Purchase!</h2>" +
                  $"<p>Dear {member.FirstName},</p>" +
                  $"<p>Your online subscription purchase has been confirmed and your subscription is now active.</p>" +
                  $"<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;'>" +
                  $"<tr><td><strong>Plan</strong></td><td>{plan.Name}</td></tr>" +
                  $"<tr><td><strong>Amount Paid</strong></td><td>{plan.Price} DZD</td></tr>" +
                  $"<tr><td><strong>Payment Method</strong></td><td>Credit Card (Online)</td></tr>" +
                  $"<tr><td><strong>Purchase Date</strong></td><td>{DateTime.UtcNow:yyyy-MM-dd}</td></tr>" +
                  $"<tr><td><strong>Valid Until</strong></td><td>{endDateText}</td></tr>" +
                  $"</table>" +
                  $"<p>Thank you for choosing FitTech! Keep pushing your limits.</p>"
        ));

        return new OnlinePurchaseResponse(
            SubscriptionId: subscription.Id,
            PaymentId: paymentId,
            PlanName: plan.Name,
            Amount: plan.Price,
            PaymentMethod: PaymentMethod.CreditCard.ToString(),
            PurchasedAt: DateTime.UtcNow,
            ValidUntil: endOnUtc
        );
    }
}
