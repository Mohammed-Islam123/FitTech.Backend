using Membership.Infrastructure;

namespace Membership.Features.Me.Payments;

public record MePaymentResponse(
    Guid PaymentId,
    decimal Amount,
    string PaymentMethod,
    string PaymentType,
    Guid ReferenceId,
    string Status,
    DateTime CreatedAt
);
