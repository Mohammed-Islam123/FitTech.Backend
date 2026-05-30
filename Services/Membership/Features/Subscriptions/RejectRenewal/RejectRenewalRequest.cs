namespace Membership.Features.Subscriptions.RejectRenewal;

public record RejectRenewalRequest(
    Guid RequestId,
    string? Reason = null
);
