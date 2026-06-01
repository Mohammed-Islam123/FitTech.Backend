using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Shared.Enums;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Features.Payments.ConfirmPayment;

/// <description>
/// Confirms a pending online payment, transitioning it to Paid status.
/// Simulates the webhook callback from an external payment gateway.
/// Called internally by the Membership service after creating a payment intent.
/// </description>
public class ConfirmPaymentHandler(PaymentDbContext context)
{
    public async Task<ErrorOr<ConfirmPaymentResponse>> Handle(
        ConfirmPaymentCommand command,
        CancellationToken ct)
    {
        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, ct);

        if (payment is null)
            return Error.NotFound("Payment.NotFound", "Payment not found.");

        if (payment.Status == PaymentStatus.Paid)
            return new ConfirmPaymentResponse(payment.Id, payment.Status);

        payment.Confirm();
        await context.SaveChangesAsync(ct);

        return new ConfirmPaymentResponse(payment.Id, payment.Status);
    }
}
