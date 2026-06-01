namespace Membership.Features.Subscriptions.ListRenewalRequests;

/// <description>
/// Summary of a pending cash renewal request for the admin dashboard.
/// </description>
public record ListRenewalRequestsResponse(
    Guid RequestId,
    Guid MemberId,
    string MemberName,
    Guid SubscriptionId,
    string PlanName,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    string? Notes
);
