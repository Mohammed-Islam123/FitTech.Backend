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

namespace Membership.Features.Subscriptions.AcceptRenewal;

/// <description>
/// Admin accepts a cash renewal request. Creates a Payment record in the Payment service,
/// creates a new extended subscription, expires the old one, and publishes a thank-you email
/// with full transaction details.
/// </description>
public class AcceptRenewalHandler(
    MembershipDbContext context,
    IUserAccessor userAccessor,
    IPaymentServiceClient paymentClient,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<AcceptRenewalResponse>> Handle(
        AcceptRenewalCommand command,
        CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Request.Unauthorized", "Only Administrators can accept requests.");

        var req = command.Request;

        var request = await context.PaymentApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == req.RequestId, ct);

        if (request is null)
            return Error.NotFound("Request.NotFound", "Renewal request not found.");

        if (request.Status != PaymentApprovalRequestStatus.Pending)
            return Error.Conflict("Request.AlreadyResolved", "This request has already been resolved.");

        var existingSubscription = await context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == request.ReferenceId, ct);

        if (existingSubscription is null)
            return Error.NotFound("Subscription.NotFound", "The subscription for this renewal no longer exists.");

        // Validate amount against plan price one more time at accept stage
        if (request.Amount != existingSubscription.Plan.Price)
            return Error.Validation("Payment.AmountMismatch",
                $"The renewal amount must match the plan price of {existingSubscription.Plan.Price} DZD.");

        var paymentPayload = new CreatePaymentRequest(
            UserId: existingSubscription.Member.UserId,
            Amount: request.Amount,
            PaymentMethod: PaymentMethod.Cash.ToString(),
            PaymentType: PaymentType.Subscription.ToString(),
            ReferenceId: existingSubscription.Id,
            Notes: req.Notes ?? request.Notes);

        var paymentResponse = await paymentClient.CreatePaymentAsync(paymentPayload);

        if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
            return Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.");

        request.Status = PaymentApprovalRequestStatus.Accepted;
        request.ResolvedAt = DateTime.UtcNow;

        var newStart = existingSubscription.EndOnUTC ?? DateTime.UtcNow;
        var newEnd = existingSubscription.Plan.DurationValue.HasValue
            ? existingSubscription.Plan.DurationUnit switch
            {
                DurationUnit.Days => newStart.AddDays(existingSubscription.Plan.DurationValue.Value),
                _ => newStart.AddMonths(existingSubscription.Plan.DurationValue ?? 1)
            }
            : (DateTime?)null;

        var renewedSubscription = new Subscription
        {
            Id = Guid.CreateVersion7(),
            MemberId = existingSubscription.MemberId,
            PlanId = existingSubscription.PlanId,
            StartOnUTC = newStart,
            EndOnUTC = newEnd,
            RemainingSessions = existingSubscription.Plan.SessionCount,
            Status = SubscriptionStatus.Active,
            PaymentId = paymentResponse.Content.PaymentId,
            PaymentStatus = PaymentStatus.Paid,
            AutoRenew = existingSubscription.AutoRenew
        };

        context.Subscriptions.Add(renewedSubscription);
        existingSubscription.Status = SubscriptionStatus.Expired;

        await context.SaveChangesAsync(ct);

        var member = existingSubscription.Member;
        var plan = existingSubscription.Plan;
        var endDateText = newEnd?.ToString("yyyy-MM-dd") ?? "N/A (session-based)";

        await messageBus.PublishAsync(new SendEmailEvent(
            To: member.FirstName,
            Subject: $"Renewal Confirmed - {plan.Name}",
            Body: $"<h2>Thank You for Your Renewal!</h2>" +
                  $"<p>Dear {member.FirstName},</p>" +
                  $"<p>Your membership renewal has been confirmed and your subscription is now active.</p>" +
                  $"<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;'>" +
                  $"<tr><td><strong>Plan</strong></td><td>{plan.Name}</td></tr>" +
                  $"<tr><td><strong>Amount Paid</strong></td><td>{request.Amount} DZD</td></tr>" +
                  $"<tr><td><strong>Payment Method</strong></td><td>Cash (Hand-to-Hand)</td></tr>" +
                  $"<tr><td><strong>Renewal Date</strong></td><td>{DateTime.UtcNow:yyyy-MM-dd}</td></tr>" +
                  $"<tr><td><strong>Valid Until</strong></td><td>{endDateText}</td></tr>" +
                  $"</table>" +
                  $"<p>Thank you for choosing FitTech! Keep pushing your limits.</p>"
        ));

        return new AcceptRenewalResponse(request.Id, paymentResponse.Content.PaymentId, request.Status);
    }
}
