namespace Payment.Features.Payments.CreatePaymentIntent;

public record CreatePaymentIntentResponse(
    Guid PaymentId,
    string ClientSecret,
    string GatewayTransactionId
);
