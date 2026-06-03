namespace Membership.Features.Subscriptions.ListPurchaseRequests;

/// <description>
/// Summary of a pending cash purchase request for the admin dashboard.
/// </description>
public record ListPurchaseRequestsResponse(
    Guid RequestId,
    Guid MemberId,
    string MemberName,
    Guid PlanId,
    string PlanName,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    string? Notes
);
