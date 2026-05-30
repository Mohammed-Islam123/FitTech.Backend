using Shared.Enums;

namespace Payment.Features.Payments.CreatePaymentIntent;

public record CreatePaymentIntentRequest(
    Guid UserId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    PaymentType PaymentType,
    Guid ReferenceId,
    string? Notes = null
);
