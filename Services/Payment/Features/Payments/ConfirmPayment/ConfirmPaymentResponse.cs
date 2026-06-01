using Shared.Enums;

namespace Payment.Features.Payments.ConfirmPayment;

public record ConfirmPaymentResponse(
    Guid PaymentId,
    PaymentStatus Status
);
