using Shared.Enums;

namespace Payment.Features.Payments.GetMyPayments;

public record MemberPaymentResponse(
    Guid PaymentId,
    decimal Amount,
    string PaymentMethod,
    string PaymentType,
    Guid ReferenceId,
    PaymentStatus Status,
    DateTime CreatedAt
);
