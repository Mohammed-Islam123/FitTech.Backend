using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Gateways;
using Payment.Infrastructure.Persistence;
using Shared.Events;
using Wolverine;

namespace Payment.Features.Payments.Webhook;

/// <description>
/// Processes incoming webhook notifications from payment providers.
/// Validates the payload, updates the payment status, and publishes
/// PaymentConfirmedEvent to notify downstream services.
/// </description>
public class WebhookHandler(
    PaymentDbContext context,
    IPaymentGateway gateway,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<WebhookResponse>> Handle(
        WebhookCommand command,
        CancellationToken ct)
    {
        var req = command.Request;

        // Process the webhook through the gateway (validates signature, parses payload)
        var result = await gateway.ProcessWebhookAsync(
            req.Provider, req.Payload, req.SignatureHeader, ct);

        if (!result.IsSuccess)
        {
            return Error.Failure("Payment.WebhookFailed",
                $"Webhook processing failed for provider {req.Provider}");
        }

        // Load the payment identified by the webhook
        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == result.PaymentId, ct);

        if (payment is null)
        {
            return Error.NotFound("Payment.NotFound",
                "Payment record not found for this webhook.");
        }

        payment.Confirm(result.GatewayTransactionId);
        await context.SaveChangesAsync(ct);

        // Notify downstream services that payment was confirmed
        await messageBus.PublishAsync(new PaymentConfirmedEvent(
            PaymentId: payment.Id,
            UserId: payment.UserId,
            Amount: payment.Amount,
            Currency: payment.Currency,
            PaymentMethod: payment.PaymentMethod,
            PaymentType: payment.PaymentType,
            ReferenceId: payment.ReferenceId,
            CreatedAt: payment.CreatedAt));

        return new WebhookResponse(payment.Id, payment.Status.ToString());
    }
}
