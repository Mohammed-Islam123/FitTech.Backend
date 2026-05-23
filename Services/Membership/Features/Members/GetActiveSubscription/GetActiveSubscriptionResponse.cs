using Membership.Domain.Enums;

namespace Membership.Features.Members.GetActiveSubscription;

/// <description>
/// Response containing active subscription details.
/// </description>
/// <example>
/// <code>
/// {
///   "subscriptionId": "018e6b3b-3ba8-7bb9-a5f9-5be017f55215",
///   "planName": "Premium Annual",
///   "price": 500.0,
///   "startOnUTC": "2026-04-24T10:00:00Z",
///   "endOnUTC": "2027-04-24T10:00:00Z",
///   "status": "Active",
///   "remainingSessions": null,
///   "pausedUntil": null
/// }
/// </code>
/// </example>
public record GetActiveSubscriptionResponse(
    Guid SubscriptionId,
    string PlanName,
    decimal Price,
    DateTime StartOnUTC,
    DateTime? EndOnUTC,
    SubscriptionStatus Status,
    int? RemainingSessions,
    DateTime? PausedUntil);
