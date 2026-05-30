namespace Membership.Features.Subscriptions.AcceptRenewal;

public record AcceptRenewalResponse(
    Guid RequestId,
    Guid PaymentId,
    string Status
);
