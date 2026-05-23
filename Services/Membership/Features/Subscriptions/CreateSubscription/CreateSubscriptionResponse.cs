using Shared.Enums;

namespace Membership.Features.Subscriptions.CreateSubscription;

public record CreateSubscriptionResponse(
    Guid SubscriptionId,
    PaymentStatus PaymentStatus);
