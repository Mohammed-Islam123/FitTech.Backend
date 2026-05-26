namespace Shared.Events;

public record MembershipRenewalAcceptedEvent(
    Guid RequestId,
    Guid MemberId,
    decimal Amount,
    Guid SubscriptionId,
    DateTime AcceptedAt
);
