using Membership.Domain.Enums;

namespace Membership.Features.Members.GetMember;

public record GetMemberResponse(
    Guid MemberId,
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime JoinDate,
    MemberStatus Status,
    int NoShowWarningCount,
    ActiveSubscriptionResponse? ActiveSubscription,
    HealthProfileResponse? HealthProfile,
    string? ActiveCardUid
);

public record ActiveSubscriptionResponse(
    Guid SubscriptionId,
    string PlanName,
    DateTime StartOnUTC,
    DateTime? EndOnUTC,
    int? RemainingSessions,
    SubscriptionStatus Status
);

public record HealthProfileResponse(
    string? Objectives,
    string? MedicalRestrictions,
    DateTime LastUpdatedAt
);
