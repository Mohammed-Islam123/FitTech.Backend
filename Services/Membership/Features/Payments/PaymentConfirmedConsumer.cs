using Membership.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Events;
using Wolverine;

namespace Membership.Features.Payments;

/// <description>
/// Handles PaymentConfirmedEvent from the Payment service.
/// Activates subscriptions and sends email notifications.
/// Used for both online payments (gateway webhook) and as a safety net
/// for cash payments processed via the admin accept flow.
/// </description>
public class PaymentConfirmedConsumer(
    MembershipDbContext context,
    IMessageBus messageBus)
{
    public async Task Handle(PaymentConfirmedEvent evt)
    {
        var subscription = await context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == evt.ReferenceId);

        if (subscription is null)
        {
            // Not a subscription payment — could be course, session, etc.
            // Those services handle their own consumers.
            return;
        }

        // Skip if already processed (idempotency)
        if (subscription.PaymentStatus == PaymentStatus.Paid)
            return;

        subscription.PaymentId = evt.PaymentId;
        subscription.PaymentStatus = PaymentStatus.Paid;

        await context.SaveChangesAsync();

        // Notify the member with full context
        var member = subscription.Member;
        var plan = subscription.Plan;

        await messageBus.PublishAsync(new SendEmailEvent(
            To: member.FirstName, // resolved via Identity in production
            Subject: $"Payment Confirmed - {plan.Name}",
            Body: $"Your payment of {evt.Amount} {evt.Currency} for {plan.Name} has been confirmed."
        ));
    }
}
