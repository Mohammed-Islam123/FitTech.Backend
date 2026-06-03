namespace Membership.Features.Subscriptions.AcceptPurchase;

public record AcceptPurchaseRequest(
    Guid RequestId,
    string? Notes = null
);
