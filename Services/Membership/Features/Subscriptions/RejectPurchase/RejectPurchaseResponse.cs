namespace Membership.Features.Subscriptions.RejectPurchase;

public record RejectPurchaseResponse(
    Guid RequestId,
    string Status
);
