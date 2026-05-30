namespace Payment.Features.Payments.Webhook;

public record WebhookRequest(
    string Provider,
    string Payload,
    string SignatureHeader
);
