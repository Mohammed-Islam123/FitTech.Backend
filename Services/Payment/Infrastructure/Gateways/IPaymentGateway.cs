namespace Payment.Infrastructure.Gateways;

/// <description>
/// Abstraction for payment gateway providers (Stripe, YooKassa, etc.).
/// The real implementation is swapped in when a PSP is integrated.
/// </description>
public interface IPaymentGateway
{
    /// <description>
    /// Creates a payment intent for the online flow.
    /// Returns a client secret the frontend uses to complete the payment.
    /// </description>
    Task<PaymentIntentResult> CreateIntentAsync(
        decimal amount,
        string currency,
        Guid paymentId,
        CancellationToken ct = default);

    /// <description>
    /// Validates and processes a webhook payload from a payment provider.
    /// Returns the confirmed payment details extracted from the payload.
    /// </description>
    Task<WebhookResult> ProcessWebhookAsync(
        string provider,
        string payload,
        string signatureHeader,
        CancellationToken ct = default);
}

public record PaymentIntentResult(
    string ClientSecret,
    string GatewayTransactionId
);

public record WebhookResult(
    Guid PaymentId,
    string GatewayTransactionId,
    bool IsSuccess,
    decimal AmountPaid
);
