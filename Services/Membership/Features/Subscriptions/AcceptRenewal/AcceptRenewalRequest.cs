namespace Membership.Features.Subscriptions.AcceptRenewal;

public record AcceptRenewalRequest(
    Guid RequestId,
    string? Notes = null
);
