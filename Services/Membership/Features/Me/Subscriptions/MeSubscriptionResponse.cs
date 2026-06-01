using Membership.Domain.Enums;

namespace Membership.Features.Me.Subscriptions;

public record MeSubscriptionResponse(
    Guid SubscriptionId,
    string PlanName,
    decimal PlanPrice,
    DateTime StartOnUTC,
    DateTime? EndOnUTC,
    int? RemainingSessions,
    SubscriptionStatus Status,
    string PaymentStatus,
    bool AutoRenew
);
