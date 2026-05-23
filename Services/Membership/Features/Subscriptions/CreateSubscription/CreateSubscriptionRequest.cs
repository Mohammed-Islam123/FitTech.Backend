using Shared.Enums;

namespace Membership.Features.Subscriptions.CreateSubscription;

public record CreateSubscriptionRequest(
    Guid MemberId,
    Guid PlanId,
    PaymentMethod PaymentMethod,
    string? Notes);
