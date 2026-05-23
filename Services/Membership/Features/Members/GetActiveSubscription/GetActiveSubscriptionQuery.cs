namespace Membership.Features.Members.GetActiveSubscription;

/// <description>
/// Query to retrieve the active subscription for a member.
/// </description>
public record GetActiveSubscriptionQuery(Guid? MemberId);
