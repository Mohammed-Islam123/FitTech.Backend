using Membership.Domain.Enums;

namespace Membership.Features.Members.CreateMember;

public record CreateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    Gender Gender,
    Guid PlanId,
    string? CardUid
);
