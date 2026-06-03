namespace Membership.Features.Members.TrackMemberEntry;

public record TrackMemberEntryCommand(TrackMemberEntryRequest Request);

public record TrackMemberEntryRequest(Guid MemberId);

public record TrackMemberEntryResponse(
    bool Success,
    string MemberName,
    int? RemainingSessions,
    ActiveMembershipInfo? ActiveMembership
);

public record ActiveMembershipInfo(
    Guid SubscriptionId,
    string PlanName,
    DateTime? EndOnUTC,
    int? RemainingSessions,
    string Status
);
