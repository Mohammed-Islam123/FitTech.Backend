using Shared.Enums;

namespace Payment.Features.Payments.CreatePayment;

public record CreatePaymentRequest(
    Guid UserId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    PaymentType PaymentType,
    Guid ReferenceId,
    string? Notes);
