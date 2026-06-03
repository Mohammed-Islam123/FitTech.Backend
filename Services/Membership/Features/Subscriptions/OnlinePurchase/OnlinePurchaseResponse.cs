namespace Membership.Features.Subscriptions.OnlinePurchase;

public record OnlinePurchaseResponse(
    Guid SubscriptionId,
    Guid PaymentId,
    string PlanName,
    decimal Amount,
    string PaymentMethod,
    DateTime PurchasedAt,
    DateTime? ValidUntil
);
