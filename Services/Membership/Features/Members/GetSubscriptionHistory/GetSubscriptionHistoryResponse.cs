using Membership.Domain.Enums;

namespace Membership.Features.Members.GetSubscriptionHistory;

public record SubscriptionHistoryResponse(List<SubscriptionHistoryItem> Subscriptions);

public record SubscriptionHistoryItem(
    Guid SubscriptionId,
    string PlanName,
    DateTime StartOnUTC,
    DateTime? EndOnUTC,
    decimal Price,
    SubscriptionStatus Status,
    int? RemainingSessions,
    DateTime? CancelledAt
);
