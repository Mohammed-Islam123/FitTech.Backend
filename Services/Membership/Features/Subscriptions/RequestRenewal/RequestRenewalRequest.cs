namespace Membership.Features.Subscriptions.RequestRenewal;

public record RequestRenewalRequest(
    Guid SubscriptionId,
    decimal Amount,
    string? Notes = null
);
