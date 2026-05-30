namespace Payment.Features.Payments.Webhook;

public record WebhookResponse(
    Guid PaymentId,
    string Status
);
