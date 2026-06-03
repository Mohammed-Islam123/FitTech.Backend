namespace Membership.Features.Subscriptions.RequestPurchase;

public record RequestPurchaseRequest(
    Guid PlanId,
    string? Notes = null
);
