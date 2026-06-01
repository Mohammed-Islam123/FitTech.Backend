using Membership.Domain.Enums;

namespace Membership.Features.Subscriptions.OnlineRenewal;

public record OnlineRenewalResponse(
    Guid SubscriptionId,
    Guid PaymentId,
    string PlanName,
    decimal Amount,
    string PaymentMethod,
    DateTime RenewedAt,
    DateTime? ValidUntil
);
