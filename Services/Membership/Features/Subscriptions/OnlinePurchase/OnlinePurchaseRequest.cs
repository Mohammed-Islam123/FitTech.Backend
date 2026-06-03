namespace Membership.Features.Subscriptions.OnlinePurchase;

public record OnlinePurchaseRequest(
    Guid PlanId,
    string? Notes = null
);
