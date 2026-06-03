namespace Membership.Features.Subscriptions.AcceptPurchase;

public record AcceptPurchaseResponse(
    Guid RequestId,
    Guid PaymentId,
    string Status
);
