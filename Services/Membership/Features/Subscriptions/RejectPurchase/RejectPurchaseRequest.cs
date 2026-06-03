namespace Membership.Features.Subscriptions.RejectPurchase;

public record RejectPurchaseRequest(
    Guid RequestId,
    string? Reason = null
);
