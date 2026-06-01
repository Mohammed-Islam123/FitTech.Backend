namespace Membership.Features.Subscriptions.OnlineRenewal;

public record OnlineRenewalRequest(
    Guid SubscriptionId,
    decimal Amount,
    string? Notes = null
);
