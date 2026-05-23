using Shared.Enums;

namespace Shared.Events;

public record PaymentConfirmedEvent(
    Guid PaymentId,
    Guid UserId,
    decimal Amount,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentType PaymentType,
    Guid ReferenceId,
    DateTime CreatedAt
);
