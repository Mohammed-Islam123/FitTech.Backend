namespace Shared.Events;

public record MemberCreatedEvent(
    Guid MemberId,
    Guid UserId,
    string FullName,
    string Email,
    string GeneratedPassword,
    string AssignedPlanName
);
