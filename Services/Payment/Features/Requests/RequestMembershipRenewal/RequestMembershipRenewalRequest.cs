namespace Payment.Features.Requests.RequestMembershipRenewal;

public record RequestMembershipRenewalRequest(
    Guid SubscriptionId,
    decimal Amount,
    string? Notes
);
