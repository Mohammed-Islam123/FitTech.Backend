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
/// Admin accepts a renewal request. Creates a Payment record in the Payment service,
/// creates a new extended subscription, and publishes notification events.
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

        // Load existing subscription to get member + plan details
        var existingSubscription = await context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == request.ReferenceId, ct);

        if (existingSubscription is null)
            return Error.NotFound("Subscription.NotFound", "The subscription for this renewal no longer exists.");

        // Create payment record in Payment service
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

        // Update request status
        request.Status = PaymentApprovalRequestStatus.Accepted;
        request.ResolvedAt = DateTime.UtcNow;

        // Create a new subscription period (extending the existing one)
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

        // Mark previous subscription as expired
        existingSubscription.Status = SubscriptionStatus.Expired;

        await context.SaveChangesAsync(ct);

        // Publish email notification (Membership has all context)
        var member = existingSubscription.Member;
        var plan = existingSubscription.Plan;
        await messageBus.PublishAsync(new SendEmailEvent(
            To: member.FirstName, // Note: email should come from Identity; using placeholder for now
            Subject: $"Renewal Confirmed - {plan.Name}",
            Body: $"Your renewal for {plan.Name} has been confirmed. Payment of {request.Amount} DZD has been received."
        ));

        return new AcceptRenewalResponse(request.Id, paymentResponse.Content.PaymentId, request.Status);
    }
}
