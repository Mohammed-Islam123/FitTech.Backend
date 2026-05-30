namespace Payment.Infrastructure.Gateways;

/// <description>
/// Mock payment gateway for development and testing. Always succeeds.
/// Swap for StripeGateway or YooKassaGateway when integrating a real PSP.
/// </description>
public class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentIntentResult> CreateIntentAsync(
        decimal amount,
        string currency,
        Guid paymentId,
        CancellationToken ct = default)
    {
        var clientSecret = $"pi_mock_{paymentId:N}_secret_{Guid.NewGuid():N}";
        var transactionId = $"txn_mock_{Guid.NewGuid():N}";

        return Task.FromResult(new PaymentIntentResult(clientSecret, transactionId));
    }

    public Task<WebhookResult> ProcessWebhookAsync(
        string provider,
        string payload,
        string signatureHeader,
        CancellationToken ct = default)
    {
        // In production, you'd validate the signature, parse the payload, and extract the payment ID.
        // For mock, we simulate a successful confirmation.

        return Task.FromResult(new WebhookResult(
            PaymentId: Guid.Empty, // caller fills this from metadata in production
            GatewayTransactionId: $"txn_mock_{Guid.NewGuid():N}",
            IsSuccess: true,
            AmountPaid: 0 // caller fills this in production
        ));
    }
}
