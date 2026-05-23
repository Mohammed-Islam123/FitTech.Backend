using ErrorOr;
using Membership.Domain;
using Membership.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Membership.Features.Subscriptions.ConfirmCashPayment;

public class ConfirmCashPaymentHandler(
    MembershipDbContext context,
    IPaymentServiceClient paymentClient)
{
    public async Task<ErrorOr<ConfirmCashPaymentResponse>> Handle(
        ConfirmCashPaymentCommand command,
        CancellationToken ct)
    {
        var req = command.Request;

        var subscription = await context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == req.SubscriptionId, cancellationToken: ct);

        if (subscription is null)
        {
            return Error.NotFound("Subscription.NotFound", "Subscription does not exist.");
        }

        if (subscription.PaymentStatus != PaymentStatus.Pending)
        {
            return Error.Conflict("Subscription.AlreadyPaid", "This subscription has already been paid.");
        }

        if (req.AmountReceived != subscription.Plan.Price)
        {
            return Error.Validation("Payment.AmountMismatch",
                "Amount received does not match the subscription plan price.");
        }

        var paymentRequest = new CreatePaymentRequest(
            UserId: subscription.Member.UserId,
            Amount: req.AmountReceived,
            PaymentMethod: req.PaymentMethod.ToString(),
            PaymentType: PaymentType.Subscription.ToString(),
            ReferenceId: subscription.Id,
            Notes: req.Notes);

        var paymentResponse = await paymentClient.CreatePaymentAsync(paymentRequest);

        if (!paymentResponse.IsSuccessStatusCode || paymentResponse.Content is null)
        {
            return Error.Failure("Payment.Failed",
                "Failed to register payment with Payment Service.");
        }

        subscription.PaymentId = paymentResponse.Content.PaymentId;
        subscription.PaymentStatus = PaymentStatus.Paid;

        await context.SaveChangesAsync(ct);

        return new ConfirmCashPaymentResponse(
            subscription.Id,
            paymentResponse.Content.PaymentId,
            subscription.PaymentStatus);
    }
}
